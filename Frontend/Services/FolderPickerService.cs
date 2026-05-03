
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using ClearText.BaseTypes;
using ClearText.Interfaces;

namespace ClearText.Services;

public sealed class FolderPickerService(IUiHost host) : BaseService, IFolderPickerService
{
    private readonly Window _window = (Window)host;

    public async Task<string?> OpenFolderPickerAsync(string? startPath)
    {
        var options = new FolderPickerOpenOptions
        {
            AllowMultiple = false,
            Title = "Select folder"
        };

        if (!string.IsNullOrEmpty(startPath))
        {
            var folder = await _window.StorageProvider
                .TryGetFolderFromPathAsync(startPath);

            if (folder != null)
                options.SuggestedStartLocation = folder;
        }

        var result = await _window.StorageProvider.OpenFolderPickerAsync(options);
        return result.FirstOrDefault()?.Path.LocalPath;
    }
}

