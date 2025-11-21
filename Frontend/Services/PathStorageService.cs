using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

public class PageStorageService
{

    private string filePathStoragePath = DetermineStoragePath(); //Cant be const cant it?

    public List<string> LoadFilePaths()
    {
        if (!File.Exists(filePathStoragePath))
        {
            var defaultfilePathStoragePath = new PageConfig
            {
                Pages = new List<string>
                {
                    "C:\\Users\\Alex\\Downloads\\Test.docx" //

                }
            };

            var defaultJSON = JsonSerializer.Serialize(defaultfilePathStoragePath, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePathStoragePath, defaultJSON);
            return defaultfilePathStoragePath.Pages;

        }

        var json = File.ReadAllText(filePathStoragePath);
        var config = JsonSerializer.Deserialize<PageConfig>(json);
        return config?.Pages ?? [];
    }

    public void SaveFilePaths(IEnumerable<string> paths)
    {
        var config = new PageConfig { Pages = paths.ToList() };
        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePathStoragePath, json);
    }

    private static string DetermineStoragePath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var configDir = Path.Combine(appData, "ClearText");
        Directory.CreateDirectory(configDir);
        return Path.Combine(configDir, "pages.json");
    }
}

public class PageConfig
{
    public List<string> Pages { get; set; } = new();
}