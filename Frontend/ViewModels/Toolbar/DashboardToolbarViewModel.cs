using System;
using System.Reactive;
using System.Threading.Tasks;
using ClearText.BaseTypes.BaseViewModels;
using ClearText.Dialogs;
using ClearText.Interfaces;
using ReactiveUI;

namespace ClearText.ViewModels.Toolbar;

public class DashboardToolbarViewModel : ViewModelBase
{
    private string? _searchText;
    private readonly IToastService _toastService;
    private readonly ISettingsService _settingsService;
    private readonly IDialogService _dialogService;

    public string SearchText
    {
        get => _searchText!;
        set => this.RaiseAndSetIfChanged(ref _searchText, value);
    }

    public ReactiveCommand<Unit, Unit> SettingsCommand { get; }


    public DashboardToolbarViewModel(
        IToastService toastService,
        ISettingsService settingsService,
        IDialogService dialogService)
    {
        _toastService = toastService;
        _settingsService = settingsService;
        _dialogService = dialogService;


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
            _toastService.CreateAndShowErrorToast("Error creating document: " + e.Message);
        }
    }

    private async Task CallSettingsDialog()
    {
        var dialog = new SettingsDialogViewModel(
            _settingsService,
            _toastService);
        await _dialogService.ShowAsync(dialog);
    }
}
