using System.Threading.Tasks;
using ClearText.DataObjects;
using ClearText.Interfaces;
using Grpc.Net.Client;
using Grammar;
using System.Linq;
using ClearTextError = ClearText.DataObjects.ClearTextError;
using System.Diagnostics;
using System;
using System.Net.Http;
using ClearText.BaseTypes;
using ClearText.Exceptions;
using ClearText.Utilities;
using ClearText.Constants;

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
                await StartupAsync();
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

    private async Task StartupAsync()
    {
        Console.WriteLine("[GrammarService] StartupAsync() called");

        PythonCleanUp.KillExistingPythonServers();
        PythonCleanUp.EnsurePortFree();

        var (pythonExe, workingDir) = FilePathFinder.LoadPythonFilePath();
        Console.WriteLine($"[GrammarService] Using Python: {pythonExe}");
        Console.WriteLine($"[GrammarService] Working directory: {workingDir}");

        var psi = new ProcessStartInfo
        {
            FileName = pythonExe,
            Arguments = "grammar_server.py",
            WorkingDirectory = workingDir,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        Console.WriteLine("[GrammarService] Starting Python process...");
        _pythonProcess = Process.Start(psi) ?? throw new Exception("Failed to start Python process.");
        _pythonProcess.OutputDataReceived += (_, e) => Console.WriteLine("[PYTHON STDOUT] " + e.Data);
        _pythonProcess.ErrorDataReceived += (_, e) => Console.WriteLine("[PYTHON STDERR] " + e.Data);

        _pythonProcess.BeginOutputReadLine();
        _pythonProcess.BeginErrorReadLine();

        Console.WriteLine("[GrammarService] Waiting for server...");
        await WaitForServerAsync();
        Console.WriteLine("[GrammarService] Server responded to ping.");

        var channel = GrpcChannel.ForAddress(PythonConstants.PythonAddress);
        _client = new Grammar.GrammarService.GrammarServiceClient(channel);

        Console.WriteLine("[GrammarService] gRPC client created.");
    }


    private static async Task WaitForServerAsync()
    {
        using var http = new HttpClient();

        for (var i = 0; i < 200; i++) // retry for 20 seconds
        {
            try
            {
                using var channel = GrpcChannel.ForAddress(PythonConstants.PythonAddress);
                await new Grammar.GrammarService.GrammarServiceClient(channel)
                    .PingAsync(new Google.Protobuf.WellKnownTypes.Empty());

                return; // server ready
            }
            catch
            {
                await Task.Delay(100);
            }
        }

        throw new Exception("Python gRPC server did not start in time.");
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

        /* Demo Code*/
        Console.WriteLine("[GrammarService] Tokens returned from Python:");
        for (var i = 0; i < reply.Tokens.Count; i++)
        {
            Console.WriteLine($"  [{i}] '{reply.Tokens[i]}'");
        }

        Console.WriteLine("[GrammarService] Errors returned from Python:");
        foreach (var e in reply.Errors)
        {
            Console.WriteLine(
                $"  Type={e.Type}, Token='{e.Token}', Index={e.Index}, Suggestions=[{string.Join(", ", e.Suggestions)}]"
            );
        }
        /* Demo Code*/


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
