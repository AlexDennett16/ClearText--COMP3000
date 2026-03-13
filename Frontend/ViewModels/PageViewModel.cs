using System;
using System.IO;
using System.Reactive;
using ClearText.BaseTypes.BaseViewModels;
using ReactiveUI;
using DocumentFormat.OpenXml.Packaging;

namespace ClearText.ViewModels;

public class PageViewModel : ViewModelBase
{
    private string _filePath;

    private string _previewText = string.Empty;

    public string PreviewText
    {
        get => _previewText;
        set => this.RaiseAndSetIfChanged(ref _previewText, value);
    }

    public string FilePath
    {
        get => _filePath;
        set => this.RaiseAndSetIfChanged(ref _filePath, value);
    }

    public string Title => Path.GetFileNameWithoutExtension(FilePath);
    public ReactiveCommand<Unit, Unit> OpenEditorCommand { get; }
    public ReactiveCommand<Unit, Unit> RenameCommand { get; }
    public ReactiveCommand<Unit, Unit> DeleteCommand { get; }


    public PageViewModel(string filePath, Action<string> openEditorCallback, Action renameCallback,
        Action deleteCallback)

    {
        _filePath = filePath;

        PreviewText = ExtractDocxPreview(filePath);


        OpenEditorCommand = ReactiveCommand.Create(() => openEditorCallback(FilePath));
        RenameCommand = ReactiveCommand.Create(renameCallback);
        DeleteCommand = ReactiveCommand.Create(deleteCallback);
    }

    public static string ExtractDocxPreview(string filePath)
    {
        const int maxChars = 1000;
        try
        {
            using var doc = WordprocessingDocument.Open(filePath, false);
            if (doc.MainDocumentPart is not { Document: not null })
                throw new InvalidDataException("Invalid DOCX file structure");
            var body = doc.MainDocumentPart.Document.Body;

            if (body == null)
                return "No preview available";

            var text = body.InnerText;

            if (string.IsNullOrWhiteSpace(text))
                return "No preview available";

            return text.Length > maxChars
                ? text[..maxChars] + "…"
                : text;
        }
        catch
        {
            return "No preview available";
        }
    }
}