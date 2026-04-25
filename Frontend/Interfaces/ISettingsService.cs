using System.Collections.ObjectModel;
using ClearText.BaseTypes.BaseViewModels;
using ClearText.Enums;

namespace ClearText.Interfaces;

public interface ISettingsService
{
    int AutoSaveInterval { get; set; }
    AppTheme CurrentTheme { get; set; }
}