using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ClearText.BaseTypes.BaseViewModels;
using ClearText.Interfaces;
using ClearText.ViewModels.Toolbar;
using ReactiveUI;

namespace ClearText.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private CompositeDisposable _navigationScope = [];
    private readonly Func<string, Action, TextEditorViewModel> _editorFactory;
    private readonly Func<Action<string>, PageSelectionViewModel> _pageSelectionFactory;
    private readonly Func<DashboardToolbarViewModel> _dashboardToolbarFactory;
    private readonly Func<string, EditorToolbarViewModel> _editorToolbarFactory;
    public IToastService ToastService { get; }

    private ViewModelBase? _currentViewModel;

    public ViewModelBase CurrentViewModel
    {
        get => _currentViewModel ?? throw new InvalidOperationException("CurrentViewModel is not set");
        set
        {
            if (_currentViewModel is IDisposable disposable)
                disposable.Dispose();


            this.RaiseAndSetIfChanged(ref _currentViewModel, value);
        }
    }

    private ViewModelBase? _toolbarViewModel;

    public ViewModelBase ToolbarViewModel
    {
        get => _toolbarViewModel ?? throw new InvalidOperationException("ToolbarViewModel is not set");
        set
        {
            if (_toolbarViewModel is IDisposable disposable)
                disposable.Dispose();


            this.RaiseAndSetIfChanged(ref _toolbarViewModel, value);
        }
    }

    public MainWindowViewModel(
        Func<string, Action, TextEditorViewModel> editorFactory,
        Func<Action<string>, PageSelectionViewModel> pageSelectionFactory,
        Func<DashboardToolbarViewModel> dashboardToolbarFactory,
        Func<string, EditorToolbarViewModel> editorToolbarFactory,
        IToastService toastService
        )
    {
        _editorFactory = editorFactory;
        _pageSelectionFactory = pageSelectionFactory;
        _dashboardToolbarFactory = dashboardToolbarFactory;
        _editorToolbarFactory = editorToolbarFactory;
        ToastService = toastService;

        ToolbarViewModel = _dashboardToolbarFactory();
        var pageSelection = _pageSelectionFactory(OpenEditor);
        CurrentViewModel = pageSelection;

        ResetScope();
        WireDashboardSearch(
            (DashboardToolbarViewModel)ToolbarViewModel,
            pageSelection);

    }

    private void OpenEditor(string filePath)
    {
        ResetScope();


        ToolbarViewModel = _editorToolbarFactory(filePath);
        CurrentViewModel = _editorFactory(filePath, ReturnToMain);
    }

    private void ReturnToMain()
    {
        ResetScope();


        ToolbarViewModel = _dashboardToolbarFactory();
        CurrentViewModel = _pageSelectionFactory(OpenEditor);

        WireDashboardSearch((DashboardToolbarViewModel)ToolbarViewModel, (PageSelectionViewModel)CurrentViewModel);
    }

    //Ensure all listeners are disposed of when navigating between pages and editors to prevent memory leaks and unintended behavior.
    private void ResetScope()
    {
        _navigationScope.Dispose();
        _navigationScope = [];
    }

    private void WireDashboardSearch(
        DashboardToolbarViewModel toolbar,
        PageSelectionViewModel pageSelection)
    {
        toolbar
            .WhenAnyValue(x => x.SearchText)
            .Throttle(TimeSpan.FromMilliseconds(200))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(text => pageSelection.FilterText = text)
            .DisposeWith(_navigationScope);
    }
}