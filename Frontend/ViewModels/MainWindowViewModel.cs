using ClearText.BaseViewModels;
using ReactiveUI;

namespace ClearText.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private ViewModelBase _currentViewModel;
    public ViewModelBase CurrentViewModel
    {
        get => _currentViewModel;
        set => this.RaiseAndSetIfChanged(ref _currentViewModel, value);
    }

    public MainWindowViewModel()
    {
        var pageSelectionVM = new PageSelectionViewModel(OpenEditor);
        CurrentViewModel = pageSelectionVM;
    }

    private void OpenEditor(string filePath)
    {
        CurrentViewModel = new TextEditorViewModel(filePath, ReturnToMain);
    }

    private void ReturnToMain()
    {
        CurrentViewModel = new PageSelectionViewModel(OpenEditor);
    }
}