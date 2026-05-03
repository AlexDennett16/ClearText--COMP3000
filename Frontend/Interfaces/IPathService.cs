using System;
using System.Collections.Generic;

namespace ClearText.Interfaces;

public interface IPathService
{
  IReadOnlyList<string> PageFilePaths { get; }

  event Action? PagePathsChanged;

  void AddPage(string path);

  void DeletePage(string path);

  void RenamePage(string oldPath, string newPath);

  void TouchPage(string path);

  string GetLastUsedFolderPath();

  List<string?> GetExistingPageNames();
}