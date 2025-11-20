using ClearText.BaseViewModels;
using ReactiveUI;

namespace ClearText.ViewModels;

public class TextEditorViewModel : ViewModelBase
{
    private string _title = "Test Page";
    private string _filePath = "TODO DEFAULT NEEDED?";
    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }
    public TextEditorViewModel()
    {
    }
}