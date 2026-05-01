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
using System.IO;
using System.Threading;
using System.Timers;

// Explicit OpenXML aliases to avoid collisions with avalonia controls
using WordRun = DocumentFormat.OpenXml.Wordprocessing.Run;
using WordParagraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;
using WordText = DocumentFormat.OpenXml.Wordprocessing.Text;
using ClearText.Enums;


namespace ClearText.ViewModels;

public class TextEditorViewModel : ViewModelBase
{
    private readonly SemaphoreSlim _saveLock = new(1, 1);
    private readonly string _filePath;
    private readonly List<WordRun> _originalRuns = [];
    private readonly IToastService _toastService;
    private readonly IGrammarService _grammarService;
    private readonly IPathService _storageService;
    private readonly IDialogService _dialogService;
    private readonly IDocumentStatsService _documentStatsService;
    private readonly INavigationService _navigationService;
    private readonly IDataDisplayDialogFactory _dataDisplayDialogFactory;
    private readonly IExitDocumentDialogFactory _exitDocumentDialogFactory;
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
        IToastService toastService,
        IGrammarService grammarService,
        IPathService storageService,
        IDialogService dialogService,
        IDocumentStatsService documentStatsService,
        ISettingsService settingsService,
        INavigationService navigationService,
        IDataDisplayDialogFactory dataDisplayDialogFactory,
        IExitDocumentDialogFactory exitDocumentDialogFactory)
    {
        _filePath = filePath;
        _toastService = toastService;
        _grammarService = grammarService;
        _storageService = storageService;
        _dialogService = dialogService;
        _documentStatsService = documentStatsService;
        _navigationService = navigationService;
        _dataDisplayDialogFactory = dataDisplayDialogFactory;
        _exitDocumentDialogFactory = exitDocumentDialogFactory;
        DocumentText = LoadDocxText(filePath);
        ReturnCommand = ReactiveCommand.CreateFromTask(HandleNavigateBack);
        SaveCommand = ReactiveCommand.CreateFromTask(ManualSaveDocument);
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

    private async Task ManualSaveDocument()
    {
        try
        {
            await SaveDocxTextAsync();
            _toastService.CreateAndShowInfoToast("Document saved.");
        }
        catch (Exception ex)
        {
            _toastService.CreateAndShowErrorToast("Auto-save failed: " + ex.Message);
        }
    }

    private async void AutoSaveDocument(object? sender, ElapsedEventArgs e)
    {
        try
        {
            await SaveDocxTextAsync();
            _toastService.CreateAndShowInfoToast("Document auto-saved.");
        }
        catch (Exception ex)
        {
            _toastService.CreateAndShowErrorToast("Auto-save failed: " + ex.Message);
        }
    }

    private async Task HandleNavigateBack()
    {
        var dialog = _exitDocumentDialogFactory.Create(
            "Unsaved Changes",
            "Are you sure you want to return to the selection screen? Any unsaved changes will be lost."
        );
        var navResult = await _dialogService.ShowAsync(dialog);


        switch (navResult)
        {
            case ExitDocumentResult.SaveAndExit:
                await SaveDocxTextAsync();
                _navigationService.ShowDashboard();
                _toastService.CreateAndShowInfoToast("Document saved.");
                break;

            case ExitDocumentResult.ExitWithoutSaving:
                _navigationService.ShowDashboard();
                _toastService.CreateAndShowInfoToast("Changes discarded.");
                break;
            case ExitDocumentResult.Cancel:
            case null:
                // User cancelled, stay on the page
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private async Task SaveDocxTextAsync()
    {
        await _saveLock.WaitAsync();
        try
        {
            using var doc = WordprocessingDocument.Open(_filePath, true);
            var body = doc.MainDocumentPart?.Document?.Body
                       ?? throw new InvalidOperationException("The document body is null.");

            body.RemoveAllChildren();

            var textIndex = 0;

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
                body.Append(new WordParagraph(new WordRun(new WordText(remaining))));
            }

            doc.MainDocumentPart.Document.Save();
            _storageService.TouchPage(_filePath);
        }
        catch (IOException ex)
        {
            // Quietly log external IO error
            Debug.WriteLine($"Save skipped due to file lock: {ex.Message}");
        }
        finally
        {
            _saveLock.Release();
        }
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

    protected override void Dispose(bool disposing)
    {

        if (!disposing)
            return;

        _autoSaveTimer.Elapsed -= AutoSaveDocument;
        _autoSaveTimer.Stop();
        _autoSaveTimer.Dispose();

        base.Dispose(disposing);
    }
}