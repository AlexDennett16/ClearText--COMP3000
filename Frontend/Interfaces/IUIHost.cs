using ClearText.BaseTypes.BaseViewModels;

namespace ClearText.Interfaces;

public interface IUiHost
{
    ViewModelBase? MainContent { get; set; }
    ViewModelBase? Toolbar { get; set; }
    object? DialogViewModel { get; set; }
}