using System.IO;
using ClearText.BaseTypes.BaseViewModels;

namespace ClearText.ViewModels.Toolbar;

public class EditorToolbarViewModel(string filePath) : ViewModelBase
{
    public string FileName { get; } = Path.GetFileNameWithoutExtension(filePath);
}
