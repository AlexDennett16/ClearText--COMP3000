using System;
using System.Reactive;
using ClearText.BaseViewModels;
using ReactiveUI;
using DocumentFormat.OpenXml.Packaging;

namespace ClearText.ViewModels;

public class TextEditorViewModel : ViewModelBase
{
    public string DocumentText { get; }
    public ReactiveCommand<Unit, Unit> ReturnCommand { get; }

    public TextEditorViewModel(string filePath, Action returnCallback)
    {
        DocumentText = LoadDocxText(filePath);
        ReturnCommand = ReactiveCommand.Create(returnCallback);
    }

    private string LoadDocxText(string path)
    {
        using var doc = WordprocessingDocument.Open(path, false);
        return doc.MainDocumentPart.Document.Body.InnerText;
    }
}