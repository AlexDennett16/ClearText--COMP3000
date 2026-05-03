using System.IO;
using System.Text.Json;
using ClearText.BaseTypes;
using ClearText.Constants;
using ClearText.Enums;
using ClearText.Interfaces;
using ClearText.Utilities;

namespace ClearText.Services;

// Service responsible for managing the application settings stored in Appdata
public sealed class SettingsService : BaseService, ISettingsService
{
    private readonly string _settingsPath = FilePathFinder.GetAppDataPath(FileIOConstants.SettingsFile);
    public bool AutoSaveEnabled { get; set; } = DefaultSettingsConstants.AutoSaveEnabled;
    public int AutoSaveInterval { get; set; } = DefaultSettingsConstants.AutoSaveInterval;
    public bool AutoGrammarCheckEnabled { get; set; } = DefaultSettingsConstants.AutoGrammarCheckEnabled;
    public int AutoGrammarCheckInterval { get; set; } = DefaultSettingsConstants.AutoGrammarCheckInterval;
    public AppTheme CurrentTheme { get; set; } = DefaultSettingsConstants.DefaultTheme;

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
            AutoGrammarCheckEnabled = AutoGrammarCheckEnabled,
            AutoGrammarCheckInterval = AutoGrammarCheckInterval,
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
            AutoGrammarCheckEnabled = config.AutoGrammarCheckEnabled;
            AutoGrammarCheckInterval = config.AutoGrammarCheckInterval;
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
        public bool AutoGrammarCheckEnabled { get; init; }
        public int AutoGrammarCheckInterval { get; init; }
        public AppTheme CurrentTheme { get; init; }
    }
}