using ClearText.BaseTypes.BaseViewModels;
using ReactiveUI;

namespace ClearText.Dialogs;

public class ConfirmCancelDialogViewModel : DialogViewModelBase<bool?>
{
    public ConfirmCancelDialogViewModel(
        string title,
        string message
    )
    {
        Title = title;
        Message = message;

        ConfirmCommand = ReactiveCommand.Create(() => Close?.Invoke(true));
        CloseCommand = ReactiveCommand.Create(() => Close?.Invoke(false));
    }
}