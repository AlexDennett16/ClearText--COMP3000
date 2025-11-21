using System;
using System.Reactive;
using ClearText.BaseViewModels;
using ReactiveUI;

namespace ClearText.ViewModels;

public class PageViewModel : ViewModelBase
{
    //TODO implement Title / Use filename
    public string FilePath { get; }
    public ReactiveCommand<Unit, Unit> OpenEditorCommand { get; }


    public PageViewModel(string filePath, Action<string> openEditorCallback)
    {
        FilePath = filePath;
        OpenEditorCommand = ReactiveCommand.Create(() => openEditorCallback(FilePath));
    }
}