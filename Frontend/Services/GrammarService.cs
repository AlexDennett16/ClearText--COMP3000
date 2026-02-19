using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using ClearText.DataObjects;
using ClearText.Interfaces;

namespace ClearText.Services;

public class GrammarService(IPathService pathService) : IGrammarService
{
    private readonly string _pythonPath = pathService.LoadPythonFilePath().PythonExe;
    private readonly string _workingDirectory = pathService.LoadPythonFilePath().WorkingDirectory;
    private Process? _pythonProcess;

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
        EnsurePythonPersists();

        if (_pythonProcess == null)
            return null;

        await _pythonProcess.StandardInput.WriteLineAsync(text);
        await _pythonProcess.StandardInput.FlushAsync();

        var output = await _pythonProcess.StandardOutput.ReadLineAsync();

        //if (!string.IsNullOrWhiteSpace(output) && output.StartsWith("__PYTHON_ERROR__"))
        //{
        //    Console.WriteLine("Python error: " + output); //TODO can remove logging
        //    return null;
        //}

        Console.WriteLine("Python output: " + output);
        var result = JsonSerializer.Deserialize<ClearTextResult>(output, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Console.WriteLine("Deserialized result: " + result);
        return result;
    }
}