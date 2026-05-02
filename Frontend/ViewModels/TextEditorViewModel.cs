using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive;
using System.Threading.Tasks;
using System.Timers;
using ClearText.BaseTypes.BaseViewModels;
using ClearText.DataObjects;
using ClearText.DialogFactoriesInterfaces;
using ClearText.Enums;
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
    private readonly IDataDisplayDialogFactory _dataDisplayDialogFactory;
    private readonly IExitDocumentDialogFactory _exitDocumentDialogFactory;
    private readonly Timer _autoSaveTimer;
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
        IDialogService dialogService,
        IDocumentStatsService documentStatsService,
        ISettingsService settingsService,
        INavigationService navigationService,
        IDocumentHandlingService documentHandlingService,
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
        _dataDisplayDialogFactory = dataDisplayDialogFactory;
        _exitDocumentDialogFactory = exitDocumentDialogFactory;
        DocumentText = _documentHandlingService.LoadText(filePath);
        ReturnCommand = ReactiveCommand.CreateFromTask(HandleNavigateBack);
        SaveCommand = ReactiveCommand.CreateFromTask(ManualSaveDocument);
        AnalyseGrammarCommand = ReactiveCommand.Create(AnalyseGrammarAction);
        ShowDocumentStatsCommand = ReactiveCommand.Create(ShowDocumentStats);

        Console.WriteLine($"AutoSaveEnabled: {settingsService.AutoSaveEnabled}, AutoSaveInterval: {settingsService.AutoSaveInterval}");
        _autoSaveTimer = new Timer(settingsService.AutoSaveInterval * 60 * 1000); // Convert minutes to milliseconds
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
            await _documentHandlingService.SaveTextAsync(_filePath, DocumentText);
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
            await _documentHandlingService.SaveTextAsync(_filePath, DocumentText);
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