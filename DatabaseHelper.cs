using System.Data.SQLite;
using System.IO;

public class DatabaseHelper
{
    private static string dbPath = "settings.db";

    public static void InitializeDatabase()
    {
        if (!File.Exists(dbPath))
        {
            SQLiteConnection.CreateFile(dbPath);
            using var conn = new SQLiteConnection($"Data Source={dbPath};Version=3;");
            conn.Open();
            string sql = @"CREATE TABLE Settings (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                SettingKey TEXT NOT NULL,
                SettingValue TEXT NOT NULL
            )";
            new SQLiteCommand(sql, conn).ExecuteNonQuery();

            InsertOrUpdateSetting("scrollSpeed", "20");
            InsertOrUpdateSetting("clickThreshold", "30");
            InsertOrUpdateSetting("smoothing", "true");
        }
    }

    public static void InsertOrUpdateSetting(string key, string value)
    {
        using var conn = new SQLiteConnection($"Data Source={dbPath};Version=3;");
        conn.Open();
        string checkSql = "SELECT COUNT(*) FROM Settings WHERE SettingKey = @key";
        using var checkCmd = new SQLiteCommand(checkSql, conn);
        checkCmd.Parameters.AddWithValue("@key", key);
        long count = (long)checkCmd.ExecuteScalar();

        string sql = count > 0 ?
            "UPDATE Settings SET SettingValue = @value WHERE SettingKey = @key" :
            "INSERT INTO Settings (SettingKey, SettingValue) VALUES (@key, @value)";

        using var cmd = new SQLiteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@key", key);
        cmd.Parameters.AddWithValue("@value", value);
        cmd.ExecuteNonQuery();
    }

    public static string GetSetting(string key, string defaultValue)
    {
        using var conn = new SQLiteConnection($"Data Source={dbPath};Version=3;");
        conn.Open();
        string sql = "SELECT SettingValue FROM Settings WHERE SettingKey = @key";
        using var cmd = new SQLiteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@key", key);
        var result = cmd.ExecuteScalar();
        return result?.ToString() ?? defaultValue;
    }
}
