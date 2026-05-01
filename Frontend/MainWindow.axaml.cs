using Avalonia;
using Avalonia.Controls;
using ClearText.BaseTypes.BaseViewModels;
using ClearText.Interfaces;

namespace ClearText;

public partial class MainWindow : Window, IUiHost
{

    public IToastService ToastService { get; }

    public MainWindow(IToastService toastService)
    {
        ToastService = toastService;
        InitializeComponent();
    }

    public static readonly DirectProperty<MainWindow, ViewModelBase?> MainContentProperty =
        AvaloniaProperty.RegisterDirect<MainWindow, ViewModelBase?>(
            nameof(MainContent),
            o => o.MainContent,
            (o, v) => o.MainContent = v);

    public static readonly DirectProperty<MainWindow, ViewModelBase?> ToolbarProperty =
        AvaloniaProperty.RegisterDirect<MainWindow, ViewModelBase?>(
            nameof(Toolbar),
            o => o.Toolbar,
            (o, v) => o.Toolbar = v);

    public static readonly DirectProperty<MainWindow, object?> DialogViewModelProperty =
        AvaloniaProperty.RegisterDirect<MainWindow, object?>(
            nameof(DialogViewModel),
            o => o.DialogViewModel,
            (o, v) => o.DialogViewModel = v);

    private ViewModelBase? _mainContent;
    private ViewModelBase? _toolbar;
    private object? _dialogViewModel;

    public ViewModelBase? MainContent
    {
        get => _mainContent;
        set => SetAndRaise(MainContentProperty, ref _mainContent, value);
    }

    public ViewModelBase? Toolbar
    {
        get => _toolbar;
        set => SetAndRaise(ToolbarProperty, ref _toolbar, value);
    }

    public object? DialogViewModel
    {
        get => _dialogViewModel;
        set => SetAndRaise(DialogViewModelProperty, ref _dialogViewModel, value);
    }
}