using System;
using System.Reactive.Linq;
using ClearText.BaseTypes.BaseViewModels;
using ClearText.Enums;
using ClearText.Interfaces;
using ClearText.ViewModels.Toolbar;
using ReactiveUI;

namespace ClearText.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public IAppServices Services { get; }
    private ViewModelBase? _currentViewModel;

    public ViewModelBase CurrentViewModel
    {
        get => _currentViewModel ?? throw new InvalidOperationException("CurrentViewModel is not set");
        set => this.RaiseAndSetIfChanged(ref _currentViewModel, value);
    }

    private ToolbarMode _toolbarMode;

    public ToolbarMode ToolbarMode
    {
        get => _toolbarMode;
        set => this.RaiseAndSetIfChanged(ref _toolbarMode, value);
    }

    public ToolbarViewModel Toolbar { get; }

    public MainWindowViewModel(IAppServices services)
    {
        Services = services;
        ToolbarMode = ToolbarMode.Dashboard;
        Toolbar = new ToolbarViewModel(this);


        var pageSelection = new PageSelectionViewModel(OpenEditor, Services);
        CurrentViewModel = pageSelection;


        Toolbar.WhenAnyValue(x => x.SearchText)
            .Throttle(TimeSpan.FromMilliseconds(200))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(text => pageSelection.FilterText = text);
    }

    private void OpenEditor(string filePath)
    {
        ToolbarMode = ToolbarMode.Editor;
        CurrentViewModel = new TextEditorViewModel(filePath, ReturnToMain, Services);
    }

    private void ReturnToMain()
    {
        ToolbarMode = ToolbarMode.Dashboard;
        CurrentViewModel = new PageSelectionViewModel(OpenEditor, Services);
    }
}