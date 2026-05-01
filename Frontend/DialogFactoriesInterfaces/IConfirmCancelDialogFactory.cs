using ClearText.Dialogs;

namespace ClearText.DialogFactoriesInterfaces;

public interface IConfirmCancelDialogFactory
{
    ConfirmCancelDialogViewModel Create(string? title = null, string? message = null);
}