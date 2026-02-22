using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ClearText.DataObjects;
using ClearText.Interfaces;

namespace ClearText.Services;

public class GrammarService(IPathService pathService) : IGrammarService
{
    private readonly string _pythonPath = pathService.LoadPythonFilePath().PythonExe;
    private readonly string _workingDirectory = pathService.LoadPythonFilePath().WorkingDirectory;
    private Process? _pythonProcess;
    private readonly SemaphoreSlim _lock = new(1, 1);


    public async Task StartupAsync()
    {
        await Task.Run(() => EnsurePythonPersists());
    }

    private void EnsurePythonPersists()
    {
        if (_pythonProcess != null && !_pythonProcess.HasExited)
        {
            return; //TODO setup python error
        }

        if (!File.Exists(_pythonPath))
            throw new FileNotFoundException("Python executable not found", _pythonPath);

        if (!Directory.Exists(_workingDirectory))
            throw new DirectoryNotFoundException("Working directory not found: " + _workingDirectory);

        var psi = new ProcessStartInfo
        {
            FileName = _pythonPath,
            Arguments = "-m AI.Pipeline",
            WorkingDirectory = _workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        _pythonProcess = Process.Start(psi);
    }

    public async Task<ClearTextResult?> CheckGrammarAsync(string text)
    {
        //UI locks are in place, but ensures no crashes
        await _lock.WaitAsync();
        try
        {
            EnsurePythonPersists();

            if (_pythonProcess == null)
                return null;

            await _pythonProcess.StandardInput.WriteLineAsync(text);
            await _pythonProcess.StandardInput.FlushAsync();

            var output = await _pythonProcess.StandardOutput.ReadLineAsync();

            //Covers the null or empty cases, which likely indicates a python error
            if (string.IsNullOrWhiteSpace(output))
            {
                Console.WriteLine("Python error, Empty or Null output: " + await _pythonProcess.StandardError.ReadToEndAsync());
                return new ClearTextResult
                {
                    Errors = [],
                    Text = "",
                    Tokens = []
                }; ;
            }

            Console.WriteLine("Python output: " + output);
            var result = JsonSerializer.Deserialize<ClearTextResult>(output, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Console.WriteLine("Deserialized result: " + result);
            return result;
        }
        finally
        {
            _lock.Release();
        }
    }
}