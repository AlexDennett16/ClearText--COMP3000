using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using ClearText.BaseTypes.BaseViewModels;
using ClearText.DataObjects;
using ClearText.Interfaces;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using ReactiveUI;

namespace ClearText.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public IAppServices Services { get; }
    private ViewModelBase? _currentViewModel;

    public ViewModelBase CurrentViewModel
    {
        get => _currentViewModel ?? throw new InvalidOperationException("CurrentViewModel is not set");
        set => this.RaiseAndSetIfChanged(ref _currentViewModel, value);
    }

    public MainWindowViewModel(IAppServices services)
    {
        Services = services;
        CurrentViewModel = new PageSelectionViewModel(OpenEditor, Services);
    }

    private void OpenEditor(string filePath)
    {
        CurrentViewModel = new TextEditorViewModel(filePath, ReturnToMain, Services);


        var result = RunGrammarCheck();
    }

    private void ReturnToMain()
    {
        CurrentViewModel = new PageSelectionViewModel(OpenEditor, Services);
    }

    public async Task<ClearTextResult?> RunGrammarCheck()
    {
        var psi = new ProcessStartInfo
        {
            FileName = "C:\\Users\\Alex\\Desktop\\Projects\\Year 3\\ClearText--COMP3000\\.venv\\Scripts\\python.exe",
            Arguments = $" -m AI.Pipeline",
            WorkingDirectory = "C:\\Users\\Alex\\Desktop\\Projects\\Year 3\\ClearText--COMP3000\\Backend",
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