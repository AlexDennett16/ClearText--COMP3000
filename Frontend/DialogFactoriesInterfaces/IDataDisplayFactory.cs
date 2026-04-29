using ClearText.DataObjects;
using ClearText.Dialogs;

namespace ClearText.DialogFactoriesInterfaces;

public interface IDataDisplayDialogFactory
{
    DataDisplayDialogViewModel Create(DocumentStats stats, string? title = null);
}