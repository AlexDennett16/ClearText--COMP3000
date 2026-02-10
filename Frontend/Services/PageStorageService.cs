using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using ClearText.Interfaces;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace ClearText.Services;

public class PathService : IPathService
{
    private readonly string PageFilePathStoragePath = DeterminePageStoragePath(); //Cant be const cant it?

    public List<string> LoadPageFilePaths()
    {
        if (!File.Exists(PageFilePathStoragePath))
        {
            var defaultFilePathStoragePath = new PageConfig
            {
                Pages =
                [
                    "C:\\Users\\alex\\Downloads\\Test.docx" //TODO Change to Nothing once testing over
                ]
            };

            var defaultJSON = JsonSerializer.Serialize(defaultFilePathStoragePath,
                new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(PageFilePathStoragePath, defaultJSON);
            return defaultFilePathStoragePath.Pages;
        }

        var json = File.ReadAllText(PageFilePathStoragePath);
        var config = JsonSerializer.Deserialize<PageConfig>(json);
        return config?.Pages ?? [];
    }

    public void SavePageFilePaths(IEnumerable<string> paths)
    {
        var config = new PageConfig { Pages = paths.ToList() };
        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(PageFilePathStoragePath, json);
    }

    private static string DeterminePageStoragePath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var configDir = Path.Combine(appData, "ClearText");
        Directory.CreateDirectory(configDir);
        return Path.Combine(configDir, "pages.json");
    }

    public string CreatePageFilePath(string pageName)
    {
        var fullPath = Path.Combine("C:\\Users\\alex\\Documents", pageName + ".docx"); //TODO Add file selection dialog

        using var doc = WordprocessingDocument.Create(
            fullPath,
            WordprocessingDocumentType.Document);
        MainDocumentPart mainPart = doc.AddMainDocumentPart();
        mainPart.Document = new Document();
        mainPart.Document.AppendChild(new Body());

        mainPart.Document.Save();

        return fullPath;
    }

    public (string PythonExe, string WorkingDirectory) LoadPythonFilePath()
    {
        // 1. Find project root then main ClearText folder
        var baseDir = AppContext.BaseDirectory;
        var projectRoot = FindDirectoryUpwards(baseDir, "ClearText--COMP3000")
                          ?? throw new DirectoryNotFoundException(
                              "Could not locate project root 'ClearText--COMP3000'.");

        // 2. Build expected venv python path
        var pythonPath = Path.Combine(projectRoot, ".venv", "Scripts", "python.exe");

        if (!File.Exists(pythonPath))
            throw new FileNotFoundException(
                $"Python executable not found at: {pythonPath}\n" +
                "Ensure your venv is created and accessible.");

        // 3. Backend folder for working directory
        var backendDir = Path.Combine(projectRoot, "Backend");

        if (!Directory.Exists(backendDir))
            throw new DirectoryNotFoundException(
                $"Backend directory not found at: {backendDir}");

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
}

public class PageConfig
{
    public List<string> Pages { get; set; } = [];
}