using System;
using System.Reactive;
using ClearText.BaseTypes.BaseViewModels;
using ReactiveUI;

namespace ClearText.ViewModels;

public class PageViewModel : ViewModelBase
{
    private string _filePath;

    public string FilePath
    {
        get => _filePath;
        set => this.RaiseAndSetIfChanged(ref _filePath, value);
    }

    public string Title => System.IO.Path.GetFileNameWithoutExtension(FilePath);
    public ReactiveCommand<Unit, Unit> OpenEditorCommand { get; }
    public ReactiveCommand<Unit, Unit> RenameCommand { get; }
    public ReactiveCommand<Unit, Unit> DeleteCommand { get; }


    public PageViewModel(string filePath, Action<string> openEditorCallback, Action renameCallback,
        Action deleteCallback)

    {
        FilePath = filePath;
        OpenEditorCommand = ReactiveCommand.Create(() => openEditorCallback(FilePath));
        RenameCommand = ReactiveCommand.Create(renameCallback);
        DeleteCommand = ReactiveCommand.Create(deleteCallback);
    }
}