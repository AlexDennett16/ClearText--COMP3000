using System;
using ClearText.BaseTypes.BaseViewModels;
using ClearText.Enums;
using ReactiveUI;

namespace ClearText.ViewModels.Toolbar;

public class ToolbarViewModel : ViewModelBase
{
    private ViewModelBase? _currentToolbar;
    private string? _searchText;

    public ViewModelBase CurrentToolbar
    {
        get => _currentToolbar!;
        set => this.RaiseAndSetIfChanged(ref _currentToolbar, value);
    }

    public string SearchText
    {
        get => _searchText!;
        set => this.RaiseAndSetIfChanged(ref _searchText, value);
    }


    public ToolbarViewModel(MainWindowViewModel mainWindowViewModel)
    {
        SetToolbarMode(mainWindowViewModel.ToolbarMode);

        mainWindowViewModel
            .WhenAnyValue(x => x.ToolbarMode)
            .Subscribe(SetToolbarMode);
    }

    private void SetToolbarMode(ToolbarMode mode)
    {
        CurrentToolbar = mode switch
        {
            ToolbarMode.Dashboard => new DashboardToolbarViewModel(this),
            ToolbarMode.Editor => new EditorToolbarViewModel(this),
            _ => new DashboardToolbarViewModel(this)
        };
    }
}
