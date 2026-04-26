using System;
using System.Reactive;
using ClearText.BaseTypes.BaseViewModels;
using ClearText.Enums;
using ClearText.Interfaces;
using ReactiveUI;

namespace ClearText.Dialogs;

public class SettingsDialogViewModel : DialogViewModelBase<bool>
{
    private readonly ISettingsService _settings;
    private Array ThemeOptions { get; } = Enum.GetValues(typeof(AppTheme));

    private int AutoSaveInterval { get; set; }
    private AppTheme SelectedTheme { get; set; }

    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> CloseCommand { get; }

    public SettingsDialogViewModel(ISettingsService settingsService, IToastService toastService)
    {
        _settings = settingsService;

        // Clone values so cancel doesn't apply them
        AutoSaveInterval = settingsService.AutoSaveInterval;
        SelectedTheme = settingsService.CurrentTheme;

        SaveCommand = ReactiveCommand.Create(() =>
        {
            // Apply changes
            _settings.AutoSaveInterval = AutoSaveInterval;
            _settings.CurrentTheme = SelectedTheme;
            toastService.CreateAndShowInfoToast("Settings saved");
            Close?.Invoke(true);
        });

        CloseCommand = ReactiveCommand.Create(() => Close?.Invoke(false));
    }
}