using Avalonia.Controls;
using ClearText.ViewModels;

namespace ClearText;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();

    }
}