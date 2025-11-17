using Avalonia.Controls;
using ClearText.ViewModels;

namespace ClearText.Views;

public partial class ToolbarView : UserControl
{
    public ToolbarView()
    {
        InitializeComponent();
        DataContext = new ToolbarViewModel();
    }
}