using System;
using Avalonia;
using ClearText.BaseTypes.BaseViewModels;
using ClearText.Enums;
using ClearText.Interfaces;
using ReactiveUI;

namespace ClearText.Dialogs;

public class SettingsDialogViewModel : DialogViewModelBase<bool>
{
    public Array ThemeOptions { get; } = Enum.GetValues(typeof(AppTheme));
    private bool _autoSaveEnabled;

    public bool AutoSaveEnabled
    {
        get => _autoSaveEnabled;
        set => this.RaiseAndSetIfChanged(ref _autoSaveEnabled, value);
    }
    public int AutoSaveInterval { get; set; }
    public AppTheme SelectedTheme { get; set; }


    public SettingsDialogViewModel(
        ISettingsService settingsService,
        IToastService toastService)
    {
        var settings = settingsService;

        // Clone values so cancel doesn't apply them
        AutoSaveEnabled = settingsService.AutoSaveEnabled;
        AutoSaveInterval = settingsService.AutoSaveInterval;
        SelectedTheme = settingsService.CurrentTheme;

        ConfirmCommand = ReactiveCommand.Create(() =>
        {
            // Apply changes
            settings.AutoSaveEnabled = AutoSaveEnabled;
            settings.AutoSaveInterval = AutoSaveInterval;
            settings.CurrentTheme = SelectedTheme;
            settings.SaveSettings();
            ((App)Application.Current!).ApplyTheme(SelectedTheme);


            toastService.CreateAndShowInfoToast("Settings saved");
            Close?.Invoke(true);
        });

        CloseCommand = ReactiveCommand.Create(() => Close?.Invoke(false));
    }
}