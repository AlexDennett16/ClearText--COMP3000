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
    public SettingsConfig Config { get; private set; } = new();
    public IToastService _toastService { get; init; }
    public SettingsService(IToastService toastService)
    {
        _toastService = toastService;
        LoadSettings();
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
                Config = config;
        }
        catch
        {
            // fallback to defaults
            Config = new SettingsConfig();
            SaveSettings();
        }
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
