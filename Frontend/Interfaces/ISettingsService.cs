using System.Collections.ObjectModel;
using ClearText.BaseTypes.BaseViewModels;
using ClearText.Enums;

namespace ClearText.Interfaces;

public interface ISettingsService
{
    bool AutoSaveEnabled { get; set; }
    int AutoSaveInterval { get; set; }
    AppTheme CurrentTheme { get; set; }
}