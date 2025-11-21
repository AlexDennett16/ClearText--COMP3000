using System;
using System.Reactive;
using ClearText.BaseViewModels;
using ReactiveUI;

namespace ClearText.ViewModels;

public class PageViewModel : ViewModelBase
{
    private string _title = "Test Page";
    private string _filePath = "TODO DEFAULT NEEDED?";
    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }
    public ReactiveCommand<Unit, Unit> OpenEditorCommand
    {
        get;
        set;
    }

    public PageViewModel( Action<string> openEditorCallback)
    {
        //_filePath = filePath;
        OpenEditorCommand = ReactiveCommand.Create(() => openEditorCallback(_filePath));
    }
}