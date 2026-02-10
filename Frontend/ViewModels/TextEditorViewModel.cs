using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;
using ClearText.BaseTypes.BaseViewModels;
using ReactiveUI;
using DocumentFormat.OpenXml.Packaging;
using System.Linq;
using ClearText.DataObjects;
using ClearText.Interfaces;

// Explicit OpenXML aliases to avoid collisions with avalonia controls
using WordRun = DocumentFormat.OpenXml.Wordprocessing.Run;
using WordParagraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;
using WordText = DocumentFormat.OpenXml.Wordprocessing.Text;


namespace ClearText.ViewModels;

public class TextEditorViewModel : ViewModelBase
{
    private readonly string _filePath;
    private readonly List<WordRun> _originalRuns = [];
    private readonly IToastService _toastService;
    private readonly IGrammarService _grammarService;
    private string _documentText = string.Empty;

    public string DocumentText
    {
        get => _documentText;
        set => this.RaiseAndSetIfChanged(ref _documentText, value);
    }

    public ReactiveCommand<Unit, Unit> ReturnCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> AnalyseGrammarCommand { get; }

    private IReadOnlyList<ClearTextError>? _errors = [];
    public IReadOnlyList<ClearTextError>? Errors
    {
        get => _errors;
        private set => this.RaiseAndSetIfChanged(ref _errors, value);
    }

    public TextEditorViewModel(string filePath, Action returnCallback, IAppServices appServices)
    {
        _filePath = filePath;
        _toastService = appServices.ToastService;
        _grammarService = appServices.GrammarService;

        DocumentText = LoadDocxText(filePath);

        ReturnCommand = ReactiveCommand.Create(returnCallback);
        SaveCommand = ReactiveCommand.Create(SaveDocxText);
        AnalyseGrammarCommand = ReactiveCommand.Create(AnalyseGrammarAction);
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

        _toastService.CreateAndShowInfoToast("Document saved successfully.");
    }

    private string LoadDocxText(string path)
    {
        using var doc = WordprocessingDocument.Open(path, false);
        var body = doc.MainDocumentPart?.Document?.Body ??
                   throw new InvalidOperationException("The document body is null.");

        _originalRuns.Clear();
        var sb = new StringBuilder();
        var paragraphs = body.Elements<WordParagraph>().ToList();

        foreach (var paragraph in paragraphs)
        {
            foreach (var run in paragraph.Elements<WordRun>())
            {
                _originalRuns.Add((WordRun)run.CloneNode(true));
                sb.Append(run.InnerText);
            }
        }

        return sb.ToString();
    }

    private async void AnalyseGrammarAction()
    {
        try
        {
            var response = await _grammarService.CheckGrammarAsync(DocumentText);
            Errors = response?.Errors;
        }
        catch (Exception e)
        {
            throw e; // TODO handle exception
        }
    }
}