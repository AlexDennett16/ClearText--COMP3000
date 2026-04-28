using System.IO;
using System.Linq;
using ClearText.BaseTypes.BaseViewModels;

namespace ClearText.ViewModels.Toolbar;

public class EditorToolbarViewModel(string filePath) : ViewModelBase
{
    public string FileName { get; } = filePath.Split(Path.DirectorySeparatorChar).LastOrDefault() ?? filePath;
}
