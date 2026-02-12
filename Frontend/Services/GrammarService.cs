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

    public async Task<ClearTextResult?> CheckGrammarAsync(string text)
    {

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

        using var process = Process.Start(psi);

        if (process == null)
            return null;

        await process.StandardInput.WriteAsync(text);
        process.StandardInput.Close();

        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

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