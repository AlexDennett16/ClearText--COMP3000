using System;
using ClearText.BaseTypes.BaseViewModels;
using ClearText.Services;
using ReactiveUI;

namespace ClearText.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private ViewModelBase? _currentViewModel;
    private readonly PageStorageService _pageStorageService = new();
    private readonly DialogService _dialogService;

    private ViewModelBase? _dialogViewModel;

    public ViewModelBase? DialogViewModel
    {
        get => _dialogViewModel;
        set => this.RaiseAndSetIfChanged(ref _dialogViewModel, value);
    }

    public ViewModelBase CurrentViewModel
    {
        get => _currentViewModel ?? throw new InvalidOperationException("CurrentViewModel is not set");
        set => this.RaiseAndSetIfChanged(ref _currentViewModel, value);
    }

    public MainWindowViewModel()
    {
        _dialogService = new DialogService(this);
        var pageSelectionVM = new PageSelectionViewModel(OpenEditor, _pageStorageService, _dialogService);
        CurrentViewModel = pageSelectionVM;
    }

    private void OpenEditor(string filePath)
    {
        CurrentViewModel = new TextEditorViewModel(filePath, ReturnToMain);
    }

    private void ReturnToMain()
    {
        CurrentViewModel = new PageSelectionViewModel(OpenEditor, _pageStorageService, _dialogService);
    }
}