using System;
using System.Reactive;
using System.Threading.Tasks;
using ClearText.BaseTypes.BaseViewModels;
using ClearText.Dialogs;
using ClearText.Enums;
using ClearText.Interfaces;
using ReactiveUI;

namespace ClearText.ViewModels.Toolbar;

public class ToolbarViewModel : ViewModelBase
{
    private ViewModelBase? _currentToolbar;
    private string? _searchText;
    private IAppServices _services;

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

    public ReactiveCommand<Unit, Unit> SettingsCommand { get; }


    public ToolbarViewModel(IAppServices services, MainWindowViewModel mainWindowViewModel)
    {
        _services = services;
        SetToolbarMode(mainWindowViewModel.ToolbarMode);

        mainWindowViewModel
            .WhenAnyValue(x => x.ToolbarMode)
            .Subscribe(SetToolbarMode);

        SettingsCommand = ReactiveCommand.Create(CreateSettingsDialog);
    }

    private async void CreateSettingsDialog()
    {
        try
        {
            await CallSettingsDialog();
        }
        catch (Exception e)
        {
            _services.ToastService.CreateAndShowErrorToast("Error creating document: " + e.Message);
        }
    }

    private async Task CallSettingsDialog()
    {
        var dialog = new SettingsDialogViewModel(
            _services.SettingsService,
            _services.ToastService);
        await _services.DialogService.ShowAsync(dialog);
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
