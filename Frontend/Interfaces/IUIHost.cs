using Avalonia.Controls;

namespace ClearText.Interfaces;

public interface IUiHost
{
    Window Window { get; }
    object? DialogViewModel { get; set; }

}