using ClearText.BaseViewModels;
using ReactiveUI;

namespace ClearText.ViewModels;

public class PageViewModel : ReactiveObject
{
    private string _title = "Test Page";
    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }
}
