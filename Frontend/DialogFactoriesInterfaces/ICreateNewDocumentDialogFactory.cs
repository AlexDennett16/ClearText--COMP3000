using ClearText.Dialogs;

namespace ClearText.DialogFactoriesInterfaces;

public interface ICreateNewDocumentDialogFactory
{
    CreateNewDocumentDialogViewModel Create(string? previousFilePath = null);
}