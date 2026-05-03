using ClearText.Enums;

namespace ClearText.Interfaces;

public interface ISettingsService
{
    bool AutoSaveEnabled { get; set; }
    int AutoSaveInterval { get; set; }
    bool AutoGrammarCheckEnabled { get; set; }
    int AutoGrammarCheckInterval { get; set; }
    AppTheme CurrentTheme { get; set; }
    void SaveSettings();
}