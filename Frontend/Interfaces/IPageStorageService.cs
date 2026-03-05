using System;
using System.Collections.Generic;

namespace ClearText.Interfaces;

public interface IPathService
{
  IReadOnlyList<string> PageFilePaths { get; }

  event Action? PagePathsChanged;
  string CreatePageFilePath(string pageName);

  void AddPage(string path);

  void DeletePage(string path);

  void RenamePage(string oldPath, string newPath);

  List<string?> GetExistingPageNames();

  (string PythonExe, string WorkingDirectory) LoadPythonFilePath();
}