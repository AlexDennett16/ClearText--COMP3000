using System.Reactive;
using ClearText.BaseTypes.BaseViewModels;
using ClearText.Enums;
using ReactiveUI;

namespace ClearText.Dialogs;

public class ExitDocumentDialogViewModel : DialogViewModelBase<ExitDocumentResult?>
{

    public ReactiveCommand<Unit, Unit> SaveAndExitCommand { get; }
    public ReactiveCommand<Unit, Unit> ExitWithoutSavingCommand { get; }

    public ExitDocumentDialogViewModel(
        string title,
        string message
    )
    {
        Title = title;
        Message = message;

        SaveAndExitCommand = ReactiveCommand.Create(() => Close?.Invoke(ExitDocumentResult.SaveAndExit));
        ExitWithoutSavingCommand = ReactiveCommand.Create(() => Close?.Invoke(ExitDocumentResult.ExitWithoutSaving));
        CloseCommand = ReactiveCommand.Create(() => Close?.Invoke(ExitDocumentResult.Cancel));
    }
}