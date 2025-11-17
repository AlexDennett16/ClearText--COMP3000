using Avalonia.Controls;
using ClearText.ViewModels;

namespace ClearText.Views;

public partial class PageView : UserControl
{
    public PageView()
    {
        InitializeComponent();
        DataContext = new PageViewModel();
    }
}