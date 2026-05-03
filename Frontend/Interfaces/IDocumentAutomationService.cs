using System;

namespace ClearText.Interfaces;

public interface IDocumentAutomationService : IDisposable
{
    public event EventHandler? AutoSaveRequested;
    public event EventHandler? AutoGrammarCheckRequested;

    void Start();
    void Stop();
}