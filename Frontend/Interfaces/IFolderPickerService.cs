
using System.Threading.Tasks;

namespace ClearText.Interfaces;

public interface IFolderPickerService
{
    Task<string?> OpenFolderPickerAsync(string? startPath);
}
