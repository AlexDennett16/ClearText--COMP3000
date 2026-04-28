using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using ClearText.Constants;
using Grpc.Net.Client;

namespace ClearText.Utilities;

public class PythonStartUp
{
    public static async Task<(Process, Grammar.GrammarService.GrammarServiceClient)> StartupAsync()
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
        var pythonProcess = Process.Start(psi) ?? throw new Exception("Failed to start Python process.");
        pythonProcess.OutputDataReceived += (_, e) => Console.WriteLine("[PYTHON STDOUT] " + e.Data);
        pythonProcess.ErrorDataReceived += (_, e) => Console.WriteLine("[PYTHON STDERR] " + e.Data);

        pythonProcess.BeginOutputReadLine();
        pythonProcess.BeginErrorReadLine();

        Console.WriteLine("[GrammarService] Waiting for server...");
        await WaitForServerAsync();
        Console.WriteLine("[GrammarService] Server responded to ping.");

        var channel = GrpcChannel.ForAddress(PythonConstants.PythonAddress);
        var client = new Grammar.GrammarService.GrammarServiceClient(channel);

        Console.WriteLine("[GrammarService] gRPC client created.");
        return (pythonProcess, client);
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
}
