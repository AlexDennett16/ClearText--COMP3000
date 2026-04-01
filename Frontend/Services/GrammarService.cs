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

namespace ClearText.Services;

public class GrammarService : BaseService, IGrammarService
{
    private Grammar.GrammarService.GrammarServiceClient? _client;
    private Process? _pythonProcess;

    public GrammarService()
    {
        _ = StartupAsync();
    }

    private async Task StartupAsync()
    {
        var psi = new ProcessStartInfo
        {
            FileName = LoadPythonFilePath().PythonExe,
            Arguments = "grammar_server.py",
            WorkingDirectory = LoadPythonFilePath().WorkingDirectory,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        _pythonProcess = Process.Start(psi);
        await WaitForServerAsync();

        var channel = GrpcChannel.ForAddress("http://localhost:50051");
        _client = new Grammar.GrammarService.GrammarServiceClient(channel);
    }

    public void StartPythonServer()
    {
        //demo TODO REMOVE ME
    }

    private async Task WaitForServerAsync()
    {
        using var http = new HttpClient();

        for (var i = 0; i < 20; i++) // retry for ~2 seconds
        {
            try
            {
                using var channel = GrpcChannel.ForAddress("http://127.0.0.1:50051");
                await new Grammar.GrammarService.GrammarServiceClient(channel)
                    .CheckGrammarAsync(new GrammarRequest { Text = "" });

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
        // Build the gRPC request
        var request = new GrammarRequest
        {
            Text = text
        };

        // Call the Python grammar service
        if (_client == null)
        {
            throw new Exception("Grammar service is not ready. Please try again later."); //Make more specific exception
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