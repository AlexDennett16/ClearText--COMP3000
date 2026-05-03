using System;
using System.Timers;
using ClearText.BaseTypes;
using ClearText.DataObjects;
using ClearText.Interfaces;
using DocumentFormat.OpenXml.Wordprocessing;

namespace ClearText.Services;

// Service responsible for managing document automation features like auto-saving and auto grammar checking.
public sealed class DocumentAutomationService : BaseService, IDocumentAutomationService
{

    public event EventHandler? AutoSaveRequested;
    public event EventHandler? AutoGrammarCheckRequested;

    private readonly ISettingsService _settingsConfig;


    private readonly Timer _autoSaveTimer;
    private readonly Timer _grammarCheckTimer;


    public DocumentAutomationService(
        ISettingsService settings)
    {
        _settingsConfig = settings;

        _autoSaveTimer = new Timer();
        _autoSaveTimer.Elapsed += (_, _) => AutoSaveRequested?.Invoke(this, EventArgs.Empty);

        _grammarCheckTimer = new Timer();
        _grammarCheckTimer.Elapsed += (_, _) => AutoGrammarCheckRequested?.Invoke(this, EventArgs.Empty);

        ApplySettings();
    }


    private void ApplySettings()
    {
        _autoSaveTimer.Interval =
            _settingsConfig.Config.AutoSaveInterval * 60 * 1000;

        _grammarCheckTimer.Interval =
            _settingsConfig.Config.AutoGrammarCheckInterval * 60 * 1000;
    }


    public void Start()
    {
        _autoSaveTimer.Enabled = _settingsConfig.Config.AutoSaveEnabled;
        _grammarCheckTimer.Enabled = _settingsConfig.Config.AutoGrammarCheckEnabled;
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