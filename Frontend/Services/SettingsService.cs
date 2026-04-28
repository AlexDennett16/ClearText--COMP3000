using System.IO;
using System.Text.Json;
using ClearText.Constants;
using ClearText.Enums;
using ClearText.Interfaces;
using ClearText.Utilities;

namespace ClearText.Services;

public class SettingsService : ISettingsService
{
    private readonly string _settingsPath = FilePathFinder.GetAppDataPath(FileIOConstants.SettingsFile);
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

            if (config == null) return;

            AutoSaveEnabled = config.AutoSaveEnabled;
            AutoSaveInterval = config.AutoSaveInterval;
            CurrentTheme = config.CurrentTheme;
        }
        catch
        {
            // fallback to defaults
            SaveSettings();
        }
    }

    private class SettingsConfig
    {
        public bool AutoSaveEnabled { get; init; }
        public int AutoSaveInterval { get; init; }
        public AppTheme CurrentTheme { get; init; }
    }
}