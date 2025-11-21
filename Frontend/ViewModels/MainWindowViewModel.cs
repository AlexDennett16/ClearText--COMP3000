using System;
using ClearText.BaseViewModels;
using ReactiveUI;

namespace ClearText.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private ViewModelBase? _currentViewModel;
    private readonly PageStorageService _pageStorageService = new();

    public ViewModelBase CurrentViewModel
    {
        get => _currentViewModel ?? throw new InvalidOperationException("CurrentViewModel is not set");
        set => this.RaiseAndSetIfChanged(ref _currentViewModel, value);
    }

    public MainWindowViewModel()
    {
        var pageSelectionVM = new PageSelectionViewModel(OpenEditor, _pageStorageService);
        CurrentViewModel = pageSelectionVM;
    }

    private void OpenEditor(string filePath)
    {
        CurrentViewModel = new TextEditorViewModel(filePath, ReturnToMain);
    }

    private void ReturnToMain()
    {
        CurrentViewModel = new PageSelectionViewModel(OpenEditor, _pageStorageService);
    }
}