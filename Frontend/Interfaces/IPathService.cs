using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClearText.BaseTypes;

namespace ClearText.Interfaces;

public interface IPathService : IBaseServiceInterface
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