using System;
using System.IO;
using System.Text.Json;
using ClearText.BaseTypes;
using ClearText.Constants;
using ClearText.DataObjects;
using ClearText.Interfaces;
using ClearText.Utilities;

namespace ClearText.Services;

// Service responsible for managing the application settings stored in Appdata
public sealed class SettingsService : BaseService, ISettingsService
{
    private readonly string _settingsPath = FilePathFinder.GetAppDataPath(FileIOConstants.SettingsFile);
    private readonly IToastService _toastService;
    public SettingsConfig Config { get; private set; }

    public SettingsService(IToastService toastService)
    {
        _toastService = toastService;
        Config = LoadOrCreateSettings();
        SaveSettings();
    }

    private SettingsConfig LoadOrCreateSettings()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                var json = File.ReadAllText(_settingsPath);
                var loaded = JsonSerializer.Deserialize<SettingsConfig>(json);

                if (loaded != null)
                    return loaded;

                _toastService.CreateAndShowErrorToast("Settings file was invalid. Using defaults.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error loading settings: " + ex);
            _toastService.CreateAndShowErrorToast("Failed to load settings. Using defaults.");
        }

        // Fallback value
        return new SettingsConfig();
    }

    public void SaveSettings()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath)!);

        var json = JsonSerializer.Serialize(Config, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(_settingsPath, json);
    }

    public void UpdateSettings(SettingsConfig config)
    {
        Config = config;
        SaveSettings();
        _toastService.CreateAndShowInfoToast("Settings saved");
    }

    public SettingsConfig CreateWorkingCopy()
    {
        return Config.Clone();
    }
}