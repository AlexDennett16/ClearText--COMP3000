using System.Threading.Tasks;
using ClearText.DataObjects;
using ClearText.Interfaces;
using Grpc.Net.Client;
using Grammar;
using System.Linq;
using ClearTextError = ClearText.DataObjects.ClearTextError;
using System.Diagnostics;
using System;
using System.IO;
using System.Net.Http;
using ClearText.BaseTypes;
using ClearText.Exceptions;

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

    var (pythonExe, workingDir) = LoadPythonFilePath();
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
    _pythonProcess = Process.Start(psi);

    if (_pythonProcess == null)
        throw new Exception("Failed to start Python process.");

    _pythonProcess.OutputDataReceived += (_, e) => Console.WriteLine("[PYTHON STDOUT] " + e.Data);
    _pythonProcess.ErrorDataReceived += (_, e) => Console.WriteLine("[PYTHON STDERR] " + e.Data);

    _pythonProcess.BeginOutputReadLine();
    _pythonProcess.BeginErrorReadLine();

    Console.WriteLine("[GrammarService] Waiting for server...");
    await WaitForServerAsync();
    Console.WriteLine("[GrammarService] Server responded to ping.");

    var channel = GrpcChannel.ForAddress("http://127.0.0.1:50051");
    _client = new Grammar.GrammarService.GrammarServiceClient(channel);

    Console.WriteLine("[GrammarService] gRPC client created.");
}


    private async Task WaitForServerAsync()
    {
        using var http = new HttpClient();

        for (var i = 0; i < 200; i++) // retry for 20 seconds
        {
            try
            {
                using var channel = GrpcChannel.ForAddress("http://127.0.0.1:50051");
                await new Grammar.GrammarService.GrammarServiceClient(channel)
                    .PingAsync(new Google.Protobuf.WellKnownTypes.Empty());

                return; //server ready
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

        // Build the gRPC request
        var request = new GrammarRequest
        {
            Text = text
        };

        // Call the Python grammar service
        if (_client == null)
        {
            throw new GrammarServiceUnavailableException(StartupError ?? new Exception("Grammar service client not initialized."));
        }

        var reply = await _client.CheckGrammarAsync(request);

        // Convert the response into existing ClearTextResult
        return new ClearTextResult
        {
            Text = reply.CorrectedText,
            Errors = reply.Errors.Select(e => new ClearTextError
            {
                Type = e.Type,
                Token = e.Token,
                Index = e.Index,
                Suggestions = e.Suggestions.ToList()
            }).ToList(),
            Tokens = [.. reply.Tokens]
        };
    }

    private static (string PythonExe, string WorkingDirectory) LoadPythonFilePath()
    {
        var baseDir = AppContext.BaseDirectory;
        var projectRoot = FindDirectoryUpwards(baseDir, "ClearText--COMP3000")
                          ?? throw new DirectoryNotFoundException("Could not locate project root.");

        var pythonPath = Path.Combine(projectRoot, ".venv", "Scripts", "python.exe");
        if (!File.Exists(pythonPath))
            throw new FileNotFoundException($"Python executable not found at: {pythonPath}");

        var backendDir = Path.Combine(projectRoot, "Backend");
        if (!Directory.Exists(backendDir))
            throw new DirectoryNotFoundException($"Backend directory not found at: {backendDir}");

        return (pythonPath, backendDir);
    }

    private static string? FindDirectoryUpwards(string startDir, string targetFolderName)
    {
        var dir = new DirectoryInfo(startDir);

        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, targetFolderName);
            if (Directory.Exists(candidate))
                return candidate;

            dir = dir.Parent;
        }

        return null;
    }

    public override void Dispose()
    {
        if (_pythonProcess is { HasExited: false })
        {
            _pythonProcess.Kill(entireProcessTree: true);
            _pythonProcess.Dispose();
        }
    }
}