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
using System.Threading.Tasks;
using ClearText.DataObjects;
using ClearText.DialogFactoriesInterfaces;

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
    private readonly IPathService _storageService;
    private readonly IDialogService _dialogService;
    private readonly IDocumentStatsService _documentStatsService;
    private readonly IDataDisplayDialogFactory _dataDisplayDialogFactory;
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
    private IReadOnlyList<string> _tokens = [];
    public IReadOnlyList<string> Tokens
    {
        get => _tokens;
        private set => this.RaiseAndSetIfChanged(ref _tokens, value);
    }

    public IReadOnlyList<ClearTextError>? Errors
    {
        get => _errors;
        private set => this.RaiseAndSetIfChanged(ref _errors, value);
    }

    public TextEditorViewModel(
        string filePath,
        Action returnCallback,
        IToastService toastService,
        IGrammarService grammarService,
        IPathService storageService,
        IDialogService dialogService,
        IDocumentStatsService documentStatsService,
        ISettingsService settingsService,
        IDataDisplayDialogFactory dataDisplayDialogFactory)
    {
        _filePath = filePath;
        _toastService = toastService;
        _grammarService = grammarService;
        _storageService = storageService;
        _dialogService = dialogService;
        _documentStatsService = documentStatsService;
        _dataDisplayDialogFactory = dataDisplayDialogFactory;
        DocumentText = LoadDocxText(filePath);
        ReturnCommand = ReactiveCommand.Create(returnCallback);
        SaveCommand = ReactiveCommand.Create(ManualSaveDocument);
        AnalyseGrammarCommand = ReactiveCommand.Create(AnalyseGrammarAction);
        ShowDocumentStatsCommand = ReactiveCommand.Create(ShowDocumentStats);

        Console.WriteLine($"AutoSaveEnabled: {settingsService.AutoSaveEnabled}, AutoSaveInterval: {settingsService.AutoSaveInterval}");
        _autoSaveTimer = new System.Timers.Timer(settingsService.AutoSaveInterval * 60 * 1000); // Convert minutes to milliseconds
        _autoSaveTimer.Elapsed += AutoSaveDocument;
        _autoSaveTimer.AutoReset = true;

        if (settingsService.AutoSaveEnabled)
        {
            _autoSaveTimer.Start();
        }

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
                var response = await _grammarService.CheckGrammarAsync(DocumentText);
                sw.Stop();

                Errors = response?.Errors;
                Tokens = response?.Tokens ?? [];
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
        var dialog = _dataDisplayDialogFactory.Create(stats, "Document Statistics");
        await _dialogService.ShowAsync(dialog);
    }

    public override void Dispose()
    {
        _autoSaveTimer.Elapsed -= AutoSaveDocument;
        _autoSaveTimer.Stop();
        _autoSaveTimer.Dispose();
        GC.SuppressFinalize(this);

        base.Dispose();
    }
}