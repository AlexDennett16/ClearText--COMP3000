using System;
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

        CurrentViewModel = new PageSelectionViewModel(OpenEditor, Services);
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