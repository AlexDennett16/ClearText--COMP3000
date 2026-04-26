using System;
using System.IO;
using System.Text.Json;
using ClearText.Enums;
using ClearText.Interfaces;

namespace ClearText.Services;

public class SettingsService : ISettingsService
{
    private readonly string _settingsPath = DetermineSettingsPath();
    public bool AutoSaveEnabled { get; set; } = true;
    public int AutoSaveInterval { get; set; } = 5;

    public AppTheme CurrentTheme { get; set; } = AppTheme.Dark;

    public SettingsService()
    {
        LoadSettings();
    }

    public void SaveSettings()
    {
        var config = new SettingsConfig
        {
            AutoSaveEnabled = AutoSaveEnabled,
            AutoSaveInterval = AutoSaveInterval,
            CurrentTheme = CurrentTheme
        };

        Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath)!);

        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(_settingsPath, json);
    }

    public void LoadSettings()
    {
        try
        {
            if (!File.Exists(_settingsPath))
            {
                SaveSettings();
                return;
            }

            var json = File.ReadAllText(_settingsPath);
            var config = JsonSerializer.Deserialize<SettingsConfig>(json);

            if (config != null)
            {
                AutoSaveEnabled = config.AutoSaveEnabled;
                AutoSaveInterval = config.AutoSaveInterval;
                CurrentTheme = config.CurrentTheme;
            }
        }
        catch
        {
            // fallback to defaults
            SaveSettings();
        }
    }

    private static string DetermineSettingsPath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var configDir = Path.Combine(appData, "ClearText");
        Directory.CreateDirectory(configDir);
        return Path.Combine(configDir, "settings.json");
    }

    private class SettingsConfig
    {
        public bool AutoSaveEnabled { get; set; }
        public int AutoSaveInterval { get; set; }
        public AppTheme CurrentTheme { get; set; }
    }
}