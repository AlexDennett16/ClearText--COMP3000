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
                    "C:\\Users\\Alex\\Downloads\\Test.docx" //TODO Change to Nothing once testing over
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
        var fullPath = Path.Combine("C:\\Users\\Alex\\Documents", pageName + ".docx"); //TODO Add file selection dialog

        using var doc = WordprocessingDocument.Create(
            fullPath,
            WordprocessingDocumentType.Document);
        MainDocumentPart mainPart = doc.AddMainDocumentPart();
        mainPart.Document = new Document();
        mainPart.Document.AppendChild(new Body());

        mainPart.Document.Save();

        return fullPath;
    }

    public (string, string) LoadPythonFilePath()
    {
        //TODO Implement this properly, maybe with a separate config file or something
        return ("C:\\Users\\Alex\\Desktop\\Projects\\Year 3\\ClearText--COMP3000\\.venv\\Scripts\\python.exe", "C:\\Users\\Alex\\Desktop\\Projects\\Year 3\\ClearText--COMP3000\\Backend");

    }
}

public class PageConfig
{
    public List<string> Pages { get; set; } = [];
}