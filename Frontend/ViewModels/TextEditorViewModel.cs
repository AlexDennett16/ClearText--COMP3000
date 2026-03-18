using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;
using ClearText.BaseTypes.BaseViewModels;
using ReactiveUI;
using DocumentFormat.OpenXml.Packaging;
using System.Linq;
using ClearText.Interfaces;
using System.Diagnostics;
using System.Text.Json;
using ClearText.Dialogs;
using System.Threading.Tasks;
using ClearText.DataObjects;

// Explicit OpenXML aliases to avoid collisions with avalonia controls
using WordRun = DocumentFormat.OpenXml.Wordprocessing.Run;
using WordParagraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;
using WordText = DocumentFormat.OpenXml.Wordprocessing.Text;

namespace ClearText.ViewModels;

public class TextEditorViewModel : ViewModelBase, IDisposable
{
    private readonly string _filePath;
    private readonly List<WordRun> _originalRuns = [];
    private readonly IToastService _toastService;
    private readonly IGrammarService _grammarService;
    private readonly IPathService _storageService;
    private readonly IDialogService _dialogService;
    private readonly IDocumentStatsService _documentStatsService;
    private readonly System.Timers.Timer _autoSaveTimer;
    private string _documentText = string.Empty;
    private bool _isGrammarChecking;

    public string DocumentText
    {
        get => _documentText;
        set => this.RaiseAndSetIfChanged(ref _documentText, value);
    }

    public ReactiveCommand<Unit, Unit> ReturnCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> AnalyseGrammarCommand { get; }
    public ReactiveCommand<Unit, Task> ShowDocumentStatsCommand { get; }

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
        _storageService = appServices.PathService;
        _dialogService = appServices.DialogService;
        _documentStatsService = appServices.DocumentStatsService;

        DocumentText = LoadDocxText(filePath);

        ReturnCommand = ReactiveCommand.Create(returnCallback);
        SaveCommand = ReactiveCommand.Create(ManualSaveDocument);
        AnalyseGrammarCommand = ReactiveCommand.Create(AnalyseGrammarAction);
        ShowDocumentStatsCommand = ReactiveCommand.Create(ShowDocumentStats);

        _autoSaveTimer = new System.Timers.Timer(TimeSpan.FromMinutes(10).TotalMilliseconds);
        _autoSaveTimer.Elapsed += AutoSaveDocument;
        _autoSaveTimer.AutoReset = true;
        _autoSaveTimer.Start();

        //Run grammar check on entry to populate squigglies immediately
        AnalyseGrammarAction();
    }

    private void ManualSaveDocument()
    {
        SaveDocxText();
        _toastService.CreateAndShowInfoToast("Document saved.");
    }

    private void AutoSaveDocument(object? sender, System.Timers.ElapsedEventArgs e)
    {
        SaveDocxText();
        _toastService.CreateAndShowInfoToast("Document auto-saved.");
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
        _storageService.TouchPage(_filePath);
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
            if (_isGrammarChecking)
            {
                _toastService.CreateAndShowInfoToast("Grammar analysis already running.");
                return;
            }

            try
            {
                _isGrammarChecking = true;
                _toastService.CreateAndShowInfoToast("Analyzing grammar...");

                var sw = Stopwatch.StartNew();
                var payload = JsonSerializer.Serialize(new { text = DocumentText });
                var response = await _grammarService.CheckGrammarAsync(payload);
                sw.Stop();

                Errors = response?.Errors;
                _toastService.CreateAndShowInfoToast($"Grammar analysis took {sw.ElapsedMilliseconds}ms");
            }
            catch (Exception e)
            {
                _toastService.CreateAndShowErrorToast("Grammar analysis failed, with error: " + e.Message);
                throw;
            }
            finally
            {
                _isGrammarChecking = false;
            }
        }
        catch (Exception e)
        {
            _toastService.CreateAndShowErrorToast("An error occurred during grammar analysis: " + e.Message);
        }
    }

    private async Task ShowDocumentStats()
    {
        var stats = _documentStatsService.GetDocumentStats(DocumentText);
        var dialog = new DataDisplayDialogViewModel(stats, "Document Statistics");


        await _dialogService.ShowAsync(dialog);
    }

    public void Dispose()
    {
        _autoSaveTimer.Elapsed -= AutoSaveDocument;
        _autoSaveTimer.Stop();
        _autoSaveTimer.Dispose();
        GC.SuppressFinalize(this);
    }
}