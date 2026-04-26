using ClearText.Enums;
using ClearText.Interfaces;

namespace ClearText.Services;

public class SettingsService : ISettingsService
{
    public bool AutoSaveEnabled { get; set; } = true; // Default to enabled
    public int AutoSaveInterval { get; set; } = 5; // Default to 5 minutes

    public AppTheme CurrentTheme { get; set; } = AppTheme.Dark; // Default to Dark theme
}