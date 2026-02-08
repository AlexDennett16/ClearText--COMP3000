using System.Collections.Generic;

namespace ClearText.Interfaces;

public interface IPathService
{
  List<string> LoadPageFilePaths();

  void SavePageFilePaths(IEnumerable<string> paths);

  string CreatePageFilePath(string pageName);

  (string, string) LoadPythonFilePath(); //TODO return type is horrific, refactor this when implementing properly
}