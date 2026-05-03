using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using ClearText.BaseTypes;
using ClearText.Constants;
using ClearText.Interfaces;
using ClearText.Utilities;

namespace ClearText.Services;

public sealed class PathService : BaseService, IPathService
{
    private readonly string _storagePath;
    private readonly List<string> _cachedPaths;
    public string LastUsedFolder { get; private set; } = "";
    public event Action? PagePathsChanged;
    public IReadOnlyList<string> PageFilePaths => _cachedPaths;

    public PathService()
    {
        _storagePath = FilePathFinder.GetAppDataPath(FileIOConstants.PagesConfigFile);
        _cachedPaths = LoadOrCreate();
    }

    private List<string> LoadOrCreate()
    {

        if (!File.Exists(_storagePath))
        {
            var defaultConfig = new PageConfig
            {
                Pages = [],
                LastUsedFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            SaveConfig(defaultConfig);
            return defaultConfig.Pages;
        }

        var json = File.ReadAllText(_storagePath);
        var config = JsonSerializer.Deserialize<PageConfig>(json);

        LastUsedFolder = config?.LastUsedFolder
                          ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        return config?.Pages ?? [];
    }

    private void SaveConfig(PageConfig config)
    {
        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_storagePath, json);
    }

    private void Persist()
    {
        SaveConfig(new PageConfig
        {
            Pages = _cachedPaths.ToList(),
            LastUsedFolder = LastUsedFolder
        });

        PagePathsChanged?.Invoke();
    }

    public void AddPage(string path)
    {
        if (_cachedPaths.Contains(path))
            return;

        _cachedPaths.Insert(0, path);
        DocumentCreator.CreateDocument(path);
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

    public void TouchPage(string path)
    {
        _cachedPaths.Remove(path);
        _cachedPaths.Insert(0, path);

        Persist();
    }

    public List<string?> GetExistingPageNames()
    {
        return _cachedPaths.Select(Path.GetFileNameWithoutExtension).ToList();
    }

    public string GetLastUsedFolderPath()
    {
        return LastUsedFolder;
    }
}

public class PageConfig
{
    public List<string> Pages { get; set; } = [];
    public string? LastUsedFolder { get; set; }
}