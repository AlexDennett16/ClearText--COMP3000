using System.Collections.Generic;

namespace ClearText.Interfaces;

public interface IPageStorageService
{
  List<string> LoadFilePaths();
  
  void SaveFilePaths(IEnumerable<string> paths);

  string CreateFilePath(string pageName);
}