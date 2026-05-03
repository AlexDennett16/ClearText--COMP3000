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
    private bool _autoGrammarCheckEnabled;
    public bool AutoGrammarCheckEnabled
    {
        get => _autoGrammarCheckEnabled;
        set => this.RaiseAndSetIfChanged(ref _autoGrammarCheckEnabled, value);
    }
    public bool AutoSaveEnabled
    {
        get => _autoSaveEnabled;
        set => this.RaiseAndSetIfChanged(ref _autoSaveEnabled, value);
    }
    public int AutoSaveInterval { get; set; }
    public int AutoGrammarCheckInterval { get; set; }
    public AppTheme SelectedTheme { get; set; }


    public SettingsDialogViewModel(
        ISettingsService settingsService,
        IToastService toastService)
    {
        var settings = settingsService;

        // Clone values so cancel doesn't apply them
        AutoSaveEnabled = settingsService.AutoSaveEnabled;
        AutoSaveInterval = settingsService.AutoSaveInterval;
        AutoGrammarCheckEnabled = settingsService.AutoGrammarCheckEnabled;
        AutoGrammarCheckInterval = settingsService.AutoGrammarCheckInterval;
        SelectedTheme = settingsService.CurrentTheme;

        ConfirmCommand = ReactiveCommand.Create(() =>
        {
            // Apply changes
            settings.AutoSaveEnabled = AutoSaveEnabled;
            settings.AutoSaveInterval = AutoSaveInterval;
            settings.AutoGrammarCheckEnabled = AutoGrammarCheckEnabled;
            settings.AutoGrammarCheckInterval = AutoGrammarCheckInterval;
            settings.CurrentTheme = SelectedTheme;
            settings.SaveSettings();
            ((App)Application.Current!).ApplyTheme(SelectedTheme);


            toastService.CreateAndShowInfoToast("Settings saved");
            Close?.Invoke(true);
        });

        CloseCommand = ReactiveCommand.Create(() => Close?.Invoke(false));
    }
}