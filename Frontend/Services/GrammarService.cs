using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ClearText.BaseTypes;
using ClearText.DataObjects;
using ClearText.Exceptions;
using ClearText.Interfaces;
using ClearText.Utilities;
using Grammar;
using ClearTextError = ClearText.DataObjects.ClearTextError;

namespace ClearText.Services;

// Service responsible for communicating with the Python grammar checking service
//Is now mostly a wrapper of other services and helpers, but still manages lifecycle
public sealed class GrammarService : BaseService, IGrammarService, IPythonStartupTask
{
    public bool IsReady { get; private set; }
    public Exception? StartupError { get; private set; }

    private Grammar.GrammarService.GrammarServiceClient? _client;
    private Process? _pythonProcess;


    public Task StartInBackground()
    {
        return Task.Run(async () =>
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
            throw new GrammarServiceUnavailableException(
                StartupError ?? new Exception("Grammar service client not initialized."));


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
            Errors =
            [
                .. reply.Errors.Select(e => new ClearTextError
                {
                    Type = e.Type,
                    Token = e.Token,
                    Index = e.Index,
                    Suggestions = [.. e.Suggestions]
                })
            ],
            Tokens = [.. reply.Tokens]
        };
    }

    protected override void Dispose(bool disposing)
    {
        if (_pythonProcess is not { HasExited: false }) return;
        _pythonProcess.Kill(entireProcessTree: true);
        _pythonProcess.Dispose();
        Console.WriteLine("[GrammarService] Python process killed and disposed.");

        base.Dispose(disposing);
    }
}