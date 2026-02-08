using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using ClearText.DataObjects;
using ClearText.Interfaces;

namespace ClearText.Services;

public class GrammarService(IPathService pathService) : IGrammarService
{
    private readonly string _pythonPath = pathService.LoadPythonFilePath().Item1;
    private readonly string _workingDirectory = pathService.LoadPythonFilePath().Item2;

    public async Task<ClearTextResult?> CheckGrammarAsync(string text)
    {

        var psi = new ProcessStartInfo
        {
            FileName = _pythonPath,
            Arguments = "-m AI.Pipeline",
            WorkingDirectory = _workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi);

        if (process == null)
            return null;

        string output = await process.StandardOutput.ReadToEndAsync();
        string error = await process.StandardError.ReadToEndAsync();

        process.WaitForExit();

        if (!string.IsNullOrWhiteSpace(error))
        {
            Console.WriteLine("Python error: " + error);
            return null;
        }

        Console.WriteLine("Python output: " + output);
        var result = JsonSerializer.Deserialize<ClearTextResult>(output, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Console.WriteLine("Deserialized result: " + result);
        return result;
    }
}

internal interface IPageStorageService
{
}