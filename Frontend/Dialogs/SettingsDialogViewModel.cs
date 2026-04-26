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
    public Array ThemeOptions { get; } = Enum.GetValues(typeof(AppTheme));
    private bool _autoSaveEnabled;

    public bool AutoSaveEnabled
    {
        get => _autoSaveEnabled;
        set => this.RaiseAndSetIfChanged(ref _autoSaveEnabled, value);
    }
    public int AutoSaveInterval { get; set; }
    public AppTheme SelectedTheme { get; set; }

    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> CloseCommand { get; }

    public SettingsDialogViewModel(ISettingsService settingsService, IToastService toastService)
    {
        _settings = settingsService;

        // Clone values so cancel doesn't apply them
        AutoSaveEnabled = settingsService.AutoSaveEnabled;
        AutoSaveInterval = settingsService.AutoSaveInterval;
        SelectedTheme = settingsService.CurrentTheme;

        SaveCommand = ReactiveCommand.Create(() =>
        {
            // Apply changes
            _settings.AutoSaveEnabled = AutoSaveEnabled;
            _settings.AutoSaveInterval = AutoSaveInterval;
            _settings.CurrentTheme = SelectedTheme;
            toastService.CreateAndShowInfoToast("Settings saved");
            Close?.Invoke(true);
        });

        CloseCommand = ReactiveCommand.Create(() => Close?.Invoke(false));
    }
}