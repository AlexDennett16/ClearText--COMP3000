using System.ComponentModel;
using Avalonia.Controls;
using ClearText.Interfaces;

namespace ClearText;

public partial class MainWindow : Window, IDialogHost, INotifyPropertyChanged
{
    private object? _dialogViewModel;

    public object? DialogViewModel
    {
        get => _dialogViewModel;
        set
        {
            if (_dialogViewModel != value)
            {
                _dialogViewModel = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DialogViewModel)));
            }
        }
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new ViewModels.MainWindowViewModel(App.Services);
    }
}