using ClearText.Enums;

namespace ClearText.DataObjects;

public class SettingsConfig
{
    public bool AutoSaveEnabled { get; set; }
    public int AutoSaveInterval { get; init; }
    public bool AutoGrammarCheckEnabled { get; set; }
    public int AutoGrammarCheckInterval { get; init; }
    public AppTheme CurrentTheme { get; init; }

    public SettingsConfig Clone() => (SettingsConfig)MemberwiseClone();
}