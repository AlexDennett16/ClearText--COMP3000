using System.Reactive;
using ClearText.BaseTypes.BaseViewModels;
using ClearText.Dialogs;
using ClearText.Interfaces;
using ReactiveUI;

namespace ClearText.ViewModels.Toolbar;

public class DashboardToolbarViewModel(IDialogService dialogService) : ViewModelBase
{
    private string? _searchText;

    public string SearchText
    {
        get => _searchText!;
        set => this.RaiseAndSetIfChanged(ref _searchText, value);
    }

    public ReactiveCommand<Unit, Unit> SettingsCommand { get; } = ReactiveCommand.CreateFromTask(async () =>
            {
                await dialogService.ShowAsync<SettingsDialogViewModel, bool>();
            });
}
