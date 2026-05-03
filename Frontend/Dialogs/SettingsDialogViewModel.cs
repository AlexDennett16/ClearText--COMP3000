using System;
using Avalonia;
using ClearText.BaseTypes.BaseViewModels;
using ClearText.DataObjects;
using ClearText.Enums;
using ClearText.Interfaces;
using ReactiveUI;

namespace ClearText.Dialogs;

public class SettingsDialogViewModel : DialogViewModelBase<bool>
{
    public SettingsConfig WorkingCopy { get; init; }

    public Array ThemeOptions { get; } = Enum.GetValues(typeof(AppTheme));

    public SettingsDialogViewModel(ISettingsService settingsService)
    {
        WorkingCopy = settingsService.CreateWorkingCopy();

        ConfirmCommand = ReactiveCommand.Create(() =>
        {
            settingsService.UpdateSettings(WorkingCopy);
            ((App)Application.Current!).ApplyTheme(WorkingCopy.CurrentTheme);
            Close?.Invoke(true);
        });

        CloseCommand = ReactiveCommand.Create(() => Close?.Invoke(false));
    }
}
