using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;
using ClearText.BaseTypes.BaseViewModels;
using ReactiveUI;
using DocumentFormat.OpenXml.Packaging;
using System.Linq;

// Explicit OpenXML aliases to avoid collisions with avalonia controls
using WordRun = DocumentFormat.OpenXml.Wordprocessing.Run;
using WordParagraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;
using WordText = DocumentFormat.OpenXml.Wordprocessing.Text;


namespace ClearText.ViewModels;

public class TextEditorViewModel : ViewModelBase
{
    private readonly string _filePath;

    // Store original runs (with formatting)
    private readonly List<WordRun> _originalRuns = [];

    private string _documentText = string.Empty;

    public string DocumentText
    {
        get => _documentText;
        set => this.RaiseAndSetIfChanged(ref _documentText, value);
    }

    public ReactiveCommand<Unit, Unit> ReturnCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveCommand { get; }

    public TextEditorViewModel(string filePath, Action returnCallback)
    {
        _filePath = filePath;

        DocumentText = LoadDocxText(filePath);

        ReturnCommand = ReactiveCommand.Create(returnCallback);
        SaveCommand = ReactiveCommand.Create(SaveDocxText);
    }

    private void SaveDocxText()
    {
        using var doc = WordprocessingDocument.Open(_filePath, true);
        var body = doc.MainDocumentPart?.Document?.Body ??
                   throw new InvalidOperationException("The document body is null.");

        body.RemoveAllChildren();

        var textIndex = 0;

        // Rebuild paragraphs and runs
        foreach (var originalRun in _originalRuns)
        {
            var newRun = (WordRun)originalRun.CloneNode(true);

            var length = originalRun.InnerText.Length;
            if (textIndex + length > DocumentText.Length)
                length = DocumentText.Length - textIndex;

            if (length <= 0)
                break;

            var runText = DocumentText.Substring(textIndex, length);
            textIndex += length;

            newRun.RemoveAllChildren<WordText>();
            newRun.AppendChild(new WordText(runText));

            var paragraph = new WordParagraph();
            paragraph.Append(newRun);
            body.Append(paragraph);
        }

        if (textIndex < DocumentText.Length)
        {
            var remaining = DocumentText[textIndex..];

            var extraParagraph = new WordParagraph();
            var extraRun = new WordRun(new WordText(remaining));
            extraParagraph.Append(extraRun);
            body.Append(extraParagraph);
        }

        doc.MainDocumentPart.Document.Save();
    }

    private string LoadDocxText(string path)
    {
        using var doc = WordprocessingDocument.Open(path, false);
        var body = doc.MainDocumentPart?.Document?.Body ??
                   throw new InvalidOperationException("The document body is null.");

        _originalRuns.Clear();
        var sb = new StringBuilder();
        var paragraphs = body.Elements<WordParagraph>().ToList();

        for (var i = 0; i < paragraphs?.Count; i++)
        {
            var paragraph = paragraphs[i];

            foreach (var run in paragraph.Elements<WordRun>())
            {
                _originalRuns.Add((WordRun)run.CloneNode(true));
                sb.Append(run.InnerText);
            }
        }

        return sb.ToString();
    }
}