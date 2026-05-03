using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using ClearText.BaseTypes.BaseViewModels;
using ClearText.DataObjects;
using ClearText.DialogFactoriesInterfaces;
using ClearText.Enums;
using ClearText.Exceptions;
using ClearText.Interfaces;
using ReactiveUI;

namespace ClearText.ViewModels;

public class TextEditorViewModel : ViewModelBase
{
    private readonly string _filePath;
    private readonly IToastService _toastService;
    private readonly IGrammarService _grammarService;
    private readonly IDialogService _dialogService;
    private readonly IDocumentStatsService _documentStatsService;
    private readonly INavigationService _navigationService;
    private readonly IDocumentHandlingService _documentHandlingService;
    private readonly IDocumentAutomationService _documentAutomationService;
    private readonly IDataDisplayDialogFactory _dataDisplayDialogFactory;
    private readonly IExitDocumentDialogFactory _exitDocumentDialogFactory;
    private string _documentText = string.Empty;
    public string DocumentText
    {
        get => _documentText;
        set => this.RaiseAndSetIfChanged(ref _documentText, value);
    }

    public ReactiveCommand<Unit, Unit> ReturnCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> AnalyseGrammarCommand { get; }
    public ReactiveCommand<Unit, Task> ShowDocumentStatsCommand { get; }

    private IReadOnlyList<ClearTextError>? _filteredErrors = [];
    private IReadOnlyList<ClearTextError>? _allErrors;
    private readonly HashSet<IgnoreOnceKey> _ignoredOnce = [];
    private IReadOnlyList<string> _tokens = [];
    public IReadOnlyList<string> Tokens
    {
        get => _tokens;
        private set => this.RaiseAndSetIfChanged(ref _tokens, value);
    }

    public IReadOnlyList<ClearTextError>? FilteredErrors
    {
        get => _filteredErrors;
        private set => this.RaiseAndSetIfChanged(ref _filteredErrors, value);
    }

    public TextEditorViewModel(
        string filePath,
        IToastService toastService,
        IGrammarService grammarService,
        IDialogService dialogService,
        IDocumentStatsService documentStatsService,
        ISettingsService settingsService,
        INavigationService navigationService,
        IDocumentHandlingService documentHandlingService,
        IDocumentAutomationService documentAutomationService,
        IDataDisplayDialogFactory dataDisplayDialogFactory,
        IExitDocumentDialogFactory exitDocumentDialogFactory)
    {
        _filePath = filePath;
        _toastService = toastService;
        _grammarService = grammarService;
        _dialogService = dialogService;
        _documentStatsService = documentStatsService;
        _navigationService = navigationService;
        _documentHandlingService = documentHandlingService;
        _documentAutomationService = documentAutomationService;
        _dataDisplayDialogFactory = dataDisplayDialogFactory;
        _exitDocumentDialogFactory = exitDocumentDialogFactory;
        DocumentText = _documentHandlingService.LoadText(filePath);
        ReturnCommand = ReactiveCommand.CreateFromTask(HandleNavigateBack);
        SaveCommand = ReactiveCommand.CreateFromTask(SaveDocument);
        AnalyseGrammarCommand = ReactiveCommand.CreateFromTask(AnalyseGrammarAsync);
        ShowDocumentStatsCommand = ReactiveCommand.Create(ShowDocumentStats);

        Console.WriteLine($"AutoSaveEnabled: {settingsService.AutoSaveEnabled}, AutoSaveInterval: {settingsService.AutoSaveInterval}");

        SetUpTimers();

        //Run grammar check on entry to populate squigglies immediately
        _ = AnalyseGrammarAsync();
    }

    private async Task SaveDocument()
    {
        try
        {
            await _documentHandlingService.SaveTextAsync(_filePath, DocumentText);
            _toastService.CreateAndShowInfoToast("Document saved.");
        }
        catch (Exception ex)
        {
            _toastService.CreateAndShowErrorToast("Auto-save failed: " + ex.Message);
        }
    }

    private IReadOnlyList<ClearTextError> ApplyIgnoreOnce(
    IReadOnlyList<ClearTextError> errors)
    {
        return errors
            .Where(e =>
                !_ignoredOnce.Contains(
                    new IgnoreOnceKey(e.Token, e.Index, e.Type)))
            .ToList();
    }

    public void IgnoreOnce(ClearTextError error)
    {
        var key = new IgnoreOnceKey(error.Token, error.Index, error.Type);
        _ignoredOnce.Add(key);

        if (_allErrors != null)
            FilteredErrors = ApplyIgnoreOnce(_allErrors);
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
                await _documentHandlingService.SaveTextAsync(_filePath, DocumentText);
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

    private async Task AnalyseGrammarAsync()
    {
        try
        {
            var sw = Stopwatch.StartNew();
            var response = await _grammarService.CheckGrammarAsync(DocumentText);
            sw.Stop();
            _allErrors = response?.Errors ?? [];
            Tokens = response?.Tokens ?? [];
            FilteredErrors = ApplyIgnoreOnce(_allErrors);
            _toastService.CreateAndShowInfoToast($"Grammar analysis completed in {sw.ElapsedMilliseconds} ms, found {FilteredErrors.Count} error{(FilteredErrors.Count == 1 ? "" : "s")}."
);
        }
        catch (GrammarServiceNotReadyException)
        {
            _toastService.CreateAndShowErrorToast(
                "Grammar service is starting. Please try again in a moment.");
        }
        catch (GrammarServiceUnavailableException ex)
        {
            _toastService.CreateAndShowErrorToast(
                "Grammar service failed to start: " + ex.Message);
        }
        catch (Exception ex)
        {
            _toastService.CreateAndShowErrorToast(
                "Grammar analysis failed: " + ex.Message);
        }
    }

    private async Task ShowDocumentStats()
    {
        var stats = _documentStatsService.GetDocumentStats(DocumentText);
        var dialog = _dataDisplayDialogFactory.Create(stats, "Document Statistics");
        await _dialogService.ShowAsync(dialog);
    }


    private void OnAutoSaveRequested(object? sender, EventArgs e)
    {
        _ = SaveDocument();
    }

    private void OnAutoGrammarCheckRequested(object? sender, EventArgs e)
    {
        _ = AnalyseGrammarAsync();
    }


    private void SetUpTimers()
    {
        _documentAutomationService.AutoSaveRequested += OnAutoSaveRequested;
        _documentAutomationService.AutoGrammarCheckRequested += OnAutoGrammarCheckRequested;

        _documentAutomationService.Start();
    }

    protected override void Dispose(bool disposing)
    {
        if (!disposing) return;

        _documentAutomationService.AutoSaveRequested -= OnAutoSaveRequested;
        _documentAutomationService.AutoGrammarCheckRequested -= OnAutoGrammarCheckRequested;
        _documentAutomationService.Stop();
        _documentAutomationService.Dispose();


        base.Dispose(disposing);
    }

    public readonly record struct IgnoreOnceKey(
    string Token,
    int Index,
    string Type
    );
}