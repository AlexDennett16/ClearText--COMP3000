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
    private readonly string _storagePath = DeterminePageStoragePath();
    private readonly List<string> _cachedPaths;

    public event Action? PagePathsChanged;

    public IReadOnlyList<string> PageFilePaths => _cachedPaths;

    public PathService()
    {
        _cachedPaths = LoadOrCreate();
    }

    private List<string> LoadOrCreate()
    {
        if (!File.Exists(_storagePath))
        {
            var defaultConfig = new PageConfig
            {
                Pages =
                [
                    "C:\\Users\\alex\\Downloads\\Test.docx" // TODO remove after testing
                ]
            };

            SaveConfig(defaultConfig);
            return defaultConfig.Pages;
        }

        var json = File.ReadAllText(_storagePath);
        var config = JsonSerializer.Deserialize<PageConfig>(json);
        return config?.Pages ?? [];
    }

    private void SaveConfig(PageConfig config)
    {
        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_storagePath, json);
    }

    private void Persist()
    {
        SaveConfig(new PageConfig { Pages = _cachedPaths.ToList() });
        PagePathsChanged?.Invoke();
    }

    public void AddPage(string path)
    {
        _cachedPaths.Insert(0, path);
        Persist();
    }

    public void DeletePage(string path)
    {
        if (File.Exists(path))
            File.Delete(path);

        _cachedPaths.Remove(path);
        Persist();
    }

    public void RenamePage(string oldPath, string newPath)
    {
        if (File.Exists(oldPath))
            File.Move(oldPath, newPath);

        var index = _cachedPaths.IndexOf(oldPath);
        if (index >= 0)
            _cachedPaths[index] = newPath;

        Persist();
    }

    public string CreatePageFilePath(string pageName)
    {
        var fullPath = Path.Combine("C:\\Users\\alex\\Documents", pageName + ".docx");

        using var doc = WordprocessingDocument.Create(fullPath, WordprocessingDocumentType.Document);
        var mainPart = doc.AddMainDocumentPart();
        mainPart.Document = new Document(new Body());
        mainPart.Document.Save();

        return fullPath;
    }


    public (string PythonExe, string WorkingDirectory) LoadPythonFilePath()
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

    private static string DeterminePageStoragePath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var configDir = Path.Combine(appData, "ClearText");
        Directory.CreateDirectory(configDir);
        return Path.Combine(configDir, "pages.json");
    }
}

public class PageConfig
{
    public List<string> Pages { get; set; } = [];
}