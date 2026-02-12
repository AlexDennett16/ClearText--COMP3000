using System;
using ClearText.BaseTypes.BaseViewModels;
using ClearText.Interfaces;
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

    public MainWindowViewModel(IAppServices services)
    {
        Services = services;
        CurrentViewModel = new PageSelectionViewModel(OpenEditor, Services);
    }

    private void OpenEditor(string filePath)
    {
        CurrentViewModel = new TextEditorViewModel(filePath, ReturnToMain, Services);
    }

    private void ReturnToMain()
    {
        CurrentViewModel = new PageSelectionViewModel(OpenEditor, Services);
    }
}