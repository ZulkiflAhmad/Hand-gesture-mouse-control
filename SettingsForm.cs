using System;
using System.Windows.Forms;

public class SettingsForm : Form
{
    private TrackBar scrollTrackBar;
    private NumericUpDown clickThresholdBox;
    private CheckBox smoothingCheckbox;
    private Button saveBtn;

    public SettingsForm()
    {
        this.Text = "Settings";
        this.Size = new System.Drawing.Size(300, 300);

        Label scrollLabel = new Label() { Text = "Scroll Speed", Top = 20, Left = 20 };
        scrollTrackBar = new TrackBar() { Minimum = 5, Maximum = 100, TickFrequency = 5, Left = 20, Top = 45, Width = 200 };
        scrollTrackBar.Value = int.Parse(DatabaseHelper.GetSetting("scrollSpeed", "20"));

        Label clickLabel = new Label() { Text = "Click Threshold", Top = 90, Left = 20 };
        clickThresholdBox = new NumericUpDown() { Minimum = 10, Maximum = 100, Left = 20, Top = 115 };
        clickThresholdBox.Value = int.Parse(DatabaseHelper.GetSetting("clickThreshold", "30"));

        smoothingCheckbox = new CheckBox() { Text = "Enable Smoothing", Top = 150, Left = 20 };
        smoothingCheckbox.Checked = bool.Parse(DatabaseHelper.GetSetting("smoothing", "true"));

        saveBtn = new Button() { Text = "Save", Top = 200, Left = 20, Width = 100 };
        saveBtn.Click += SaveBtn_Click;

        this.Controls.Add(scrollLabel);
        this.Controls.Add(scrollTrackBar);
        this.Controls.Add(clickLabel);
        this.Controls.Add(clickThresholdBox);
        this.Controls.Add(smoothingCheckbox);
        this.Controls.Add(saveBtn);
    }

    private void SaveBtn_Click(object sender, EventArgs e)
    {
        DatabaseHelper.InsertOrUpdateSetting("scrollSpeed", scrollTrackBar.Value.ToString());
        DatabaseHelper.InsertOrUpdateSetting("clickThreshold", clickThresholdBox.Value.ToString());
        DatabaseHelper.InsertOrUpdateSetting("smoothing", smoothingCheckbox.Checked.ToString());

        MessageBox.Show("Settings saved!", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
        this.Close();
    }
}
