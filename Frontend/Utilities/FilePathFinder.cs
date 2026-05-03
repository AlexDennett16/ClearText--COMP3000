using System;
using System.IO;

namespace ClearText.Utilities;

public static class FilePathFinder
{
    /// Finds the AppData Folder location, creates it if it does not exist, and returns the full path to a specified file within it.
    internal static string GetAppDataPath(string fileName)
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var configDir = Path.Combine(appData, "ClearText");
        Directory.CreateDirectory(configDir);
        return Path.Combine(configDir, fileName);
    }

    //Finds the relevant Python File Paths
    //Ugly return tuple, but is only used in one place and avoids the need for a custom struct 
    internal static (string PythonExe, string WorkingDirectory) LoadPythonFilePath()
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
}