using ClearText.Dialogs;

namespace ClearText.DialogFactoriesInterfaces;

public interface IExitDocumentDialogFactory
{
    ExitDocumentDialogViewModel Create(string? title, string? message);
}