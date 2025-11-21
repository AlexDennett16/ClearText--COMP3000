using System;
using System.Reactive;
using ClearText.BaseViewModels;
using ReactiveUI;

namespace ClearText.ViewModels;

public class PageViewModel : ViewModelBase
{
    public string FilePath { get; }
    public string Title => System.IO.Path.GetFileNameWithoutExtension(FilePath);
    public ReactiveCommand<Unit, Unit> OpenEditorCommand { get; }


    public PageViewModel(string filePath, Action<string> openEditorCallback)
    {
        FilePath = filePath;
        OpenEditorCommand = ReactiveCommand.Create(() => openEditorCallback(FilePath));
    }
}