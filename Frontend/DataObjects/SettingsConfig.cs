using ClearText.Constants;
using ClearText.Enums;

namespace ClearText.DataObjects;

public class SettingsConfig
{
    public bool AutoSaveEnabled { get; set; } = DefaultSettingsConstants.AutoSaveEnabled;
    public int AutoSaveInterval { get; set; } = DefaultSettingsConstants.AutoSaveInterval;
    public bool AutoGrammarCheckEnabled { get; set; } = DefaultSettingsConstants.AutoGrammarCheckEnabled;
    public int AutoGrammarCheckInterval { get; set; } = DefaultSettingsConstants.AutoGrammarCheckInterval;
    public AppTheme CurrentTheme { get; set; } = DefaultSettingsConstants.DefaultTheme;

    public SettingsConfig Clone() => (SettingsConfig)MemberwiseClone();
}