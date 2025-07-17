using System;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

public class MainForm : Form
{
    private Button startBtn;
    private Label statusLabel;
    private bool isRunning = false;
    private bool gestureLoop = true;
    private int scrollSpeed;
    private int clickThreshold;
    private bool smoothingEnabled;

    [DllImport("user32.dll")]
    static extern void mouse_event(int flags, int dx, int dy, int data, int extraInfo);

    const int MOUSEEVENTF_LEFTDOWN = 0x02;
    const int MOUSEEVENTF_LEFTUP = 0x04;
    const int MOUSEEVENTF_RIGHTDOWN = 0x08;
    const int MOUSEEVENTF_RIGHTUP = 0x10;
    const int MOUSEEVENTF_WHEEL = 0x0800;

    public MainForm()
    {
        this.Text = "Virtual Mouse Controller";
        this.Size = new Size(400, 200);

        startBtn = new Button()
        {
            Text = "Start Gesture Mouse",
            Location = new Point(100, 50),
            Size = new Size(200, 40)
        };
        startBtn.Click += StartBtn_Click;
        this.Controls.Add(startBtn);

        statusLabel = new Label()
        {
            Text = "Status: Idle",
            Location = new Point(100, 110),
            AutoSize = true
        };
        this.Controls.Add(statusLabel);

        Button settingsBtn = new Button()
        {
            Text = "Settings",
            Location = new Point(100, 150),
            Size = new Size(200, 30)
        };
        settingsBtn.Click += (s, e) =>
        {
            new SettingsForm().ShowDialog();
        };
        this.Controls.Add(settingsBtn);

        // Load settings
        DatabaseHelper.InitializeDatabase();
        scrollSpeed = int.Parse(DatabaseHelper.GetSetting("scrollSpeed", "20"));
        clickThreshold = int.Parse(DatabaseHelper.GetSetting("clickThreshold", "30"));
        smoothingEnabled = bool.Parse(DatabaseHelper.GetSetting("smoothing", "true"));

        Console.WriteLine($"Settings Loaded: scroll={scrollSpeed}, threshold={clickThreshold}, smoothing={smoothingEnabled}");
    }

    private void StartBtn_Click(object sender, EventArgs e)
    {
        if (!isRunning)
        {
            statusLabel.Text = "Status: Running...";
            isRunning = true;
            gestureLoop = true;
            Task.Run(() => GestureMouseLogic());
        }
    }

    private void GestureMouseLogic()
    {
        UdpClient client = new UdpClient(9999);
        IPEndPoint ep = new IPEndPoint(IPAddress.Any, 9999);
        DateTime lastClick = DateTime.Now;
        int screenW = Screen.PrimaryScreen.Bounds.Width;
        int screenH = Screen.PrimaryScreen.Bounds.Height;

        int lastIndexX = 0;
        DateTime lastSwipeTime = DateTime.Now;

        try
        {
            while (gestureLoop)
            {
                byte[] data = client.Receive(ref ep);
                string json = Encoding.UTF8.GetString(data);
                dynamic landmarks = JsonConvert.DeserializeObject(json);

                int indexX = (int)(landmarks.index_x * screenW);
                int indexY = (int)(landmarks.index_y * screenH);
                int middleX = (int)(landmarks.middle_x * screenW);
                int middleY = (int)(landmarks.middle_y * screenH);
                int thumbY = (int)(landmarks.thumb_y * screenH);

                Cursor.Position = new Point(indexX, indexY);

                // Regular click gesture (index and middle fingers close)
                if (Math.Abs(indexX - middleX) < clickThreshold && Math.Abs(indexY - middleY) < clickThreshold)
                {
                    if ((DateTime.Now - lastClick).TotalMilliseconds > 500)
                    {
                        mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                        lastClick = DateTime.Now;
                    }
                }

                // Peace gesture (double-click + minimize)
                bool peaceGesture = Math.Abs(indexX - middleX) > (clickThreshold + 50) &&
                                    indexY < thumbY && middleY < thumbY;

                if (peaceGesture)
                {
                    if ((DateTime.Now - lastClick).TotalMilliseconds > 800)
                    {
                        mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                        System.Threading.Thread.Sleep(100);
                        mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);

                        this.Invoke((MethodInvoker)delegate
                        {
                            this.WindowState = FormWindowState.Minimized;
                        });

                        lastClick = DateTime.Now;
                    }
                }

                // Scroll gesture
                int diff = Math.Abs(indexY - thumbY);
                if (diff < clickThreshold)
                    mouse_event(MOUSEEVENTF_WHEEL, 0, 0, -scrollSpeed, 0);
                else if (diff > (scrollSpeed + 20))
                    mouse_event(MOUSEEVENTF_WHEEL, 0, 0, scrollSpeed, 0);

                // Motion gesture: horizontal swipe for left/right click
                int swipeDiff = indexX - lastIndexX;
                TimeSpan swipeDelta = DateTime.Now - lastSwipeTime;

                if (swipeDelta.TotalMilliseconds > 100)
                {
                    if (swipeDiff > 100) // right swipe
                    {
                        mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
                        mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
                        lastSwipeTime = DateTime.Now;
                    }
                    else if (swipeDiff < -100) // left swipe
                    {
                        mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                        lastSwipeTime = DateTime.Now;
                    }
                }

                lastIndexX = indexX;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("UDP Error: " + ex.Message);
        }
        finally
        {
            client.Close();
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        gestureLoop = false;
        isRunning = false;
        base.OnFormClosing(e);
    }

}