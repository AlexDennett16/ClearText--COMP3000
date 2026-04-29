using ClearText.Dialogs;

namespace ClearText.DialogFactoriesInterfaces;

public interface IStringDialogFactory
{
    StringDialogViewModel Create(string? startingMessage = null);
}