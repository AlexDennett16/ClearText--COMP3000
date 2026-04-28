using System.Threading.Tasks;
using ClearText.DataObjects;
using ClearText.Interfaces;
using Grammar;
using System.Linq;
using ClearTextError = ClearText.DataObjects.ClearTextError;
using System.Diagnostics;
using System;
using ClearText.BaseTypes;
using ClearText.Exceptions;
using ClearText.Utilities;

namespace ClearText.Services;

public class GrammarService : BaseService, IGrammarService
{
    public bool IsReady { get; private set; }
    public Exception? StartupError { get; private set; }

    private Grammar.GrammarService.GrammarServiceClient? _client;
    private Process? _pythonProcess;



    public GrammarService()
    {
        StartInBackground();
    }

    private void StartInBackground()
    {
        Task.Run(async () =>
        {
            try
            {
                (_pythonProcess, _client) = await PythonStartUp.StartupAsync();
                IsReady = true;
                Console.WriteLine("GrammarService ready");
            }
            catch (Exception ex)
            {
                StartupError = ex;
                Console.WriteLine("GrammarService failed: " + ex);
            }
        });
    }

    public async Task<ClearTextResult?> CheckGrammarAsync(string text)
    {
        if (!IsReady)
        {
            throw new GrammarServiceNotReadyException();
        }

        if (_client == null)
            throw new GrammarServiceUnavailableException(StartupError ?? new Exception("Grammar service client not initialized."));


        var tokens = TextTokeniser.TokeniseOnWhitespace(text);

        var reply = await _client.CheckGrammarAsync(
            new GrammarRequest
            {
                Tokens = { tokens.Select(t => t.Text) }
            }
        );


        return new ClearTextResult
        {
            Text = reply.CorrectedText,
            Errors = [.. reply.Errors.Select(e => new ClearTextError
            {
                Type = e.Type,
                Token = e.Token,
                Index = e.Index,
                Suggestions = [.. e.Suggestions]
            })],
            Tokens = [.. reply.Tokens]


        };
    }

    public override void Dispose()
    {
        if (_pythonProcess is not { HasExited: false }) return;
        _pythonProcess.Kill(entireProcessTree: true);
        _pythonProcess.Dispose();
        Console.WriteLine("[GrammarService] Python process killed and disposed.");
    }
}
