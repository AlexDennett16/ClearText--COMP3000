using System;
using System.Timers;
using ClearText.BaseTypes;
using ClearText.Interfaces;

namespace ClearText.Services;

public sealed class DocumentAutomationService : BaseService, IDocumentAutomationService
{

    public event EventHandler? AutoSaveRequested;
    public event EventHandler? AutoGrammarCheckRequested;

    private readonly ISettingsService _settings;


    private readonly Timer _autoSaveTimer;
    private readonly Timer _grammarCheckTimer;


    public DocumentAutomationService(
        ISettingsService settings)
    {
        _settings = settings;

        _autoSaveTimer = new Timer();
        _autoSaveTimer.Elapsed += (_, _) => AutoSaveRequested?.Invoke(this, EventArgs.Empty);

        _grammarCheckTimer = new Timer();
        _grammarCheckTimer.Elapsed += (_, _) => AutoGrammarCheckRequested?.Invoke(this, EventArgs.Empty);

        ApplySettings();
    }


    private void ApplySettings()
    {
        _autoSaveTimer.Interval =
            _settings.AutoSaveInterval * 60 * 1000;

        _grammarCheckTimer.Interval =
            _settings.AutoGrammarCheckInterval * 60 * 1000;
    }


    public void Start()
    {
        _autoSaveTimer.Enabled = _settings.AutoSaveEnabled;
        _grammarCheckTimer.Enabled = _settings.AutoGrammarCheckEnabled;
    }

    public void Stop()
    {
        _autoSaveTimer.Enabled = false;
        _grammarCheckTimer.Enabled = false;
    }



    protected override void Dispose(bool disposing)
    {

        if (!disposing)
            return;

        Stop();

        _autoSaveTimer.Dispose();
        _grammarCheckTimer.Dispose();
        base.Dispose(disposing);
    }
}