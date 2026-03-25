using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using ClearText.BaseTypes.BaseViewModels;
using ClearText.Interfaces;
using ReactiveUI;

namespace ClearText.Dialogs;

public class CreateNewDocumentDialogViewModel : DialogViewModelBase<string?>
{
    private string? _documentName;
    private string? _filePath;
    private readonly IToastService _toastService;
    private readonly IPathService _pathService;

    public string? DocumentName
    {
        get => _documentName;
        set => this.RaiseAndSetIfChanged(ref _documentName, value);
    }

    public string? FilePath
    {
        get => _filePath;
        set => this.RaiseAndSetIfChanged(ref _filePath, value);
    }

    public ReactiveCommand<Unit, Unit> Confirm { get; }
    public ReactiveCommand<Unit, Unit> Cancel { get; }

    public ReactiveCommand<Unit, Task> Browse { get; }

    public CreateNewDocumentDialogViewModel(IToastService toastService, IPathService pathService, string previousFilePath = "")
    {
        _toastService = toastService;
        _pathService = pathService;
        FilePath = previousFilePath;

        Title = "Please enter a Document name and choose a folder";

        Confirm = ReactiveCommand.Create(ExecuteConfirm);
        Cancel = ReactiveCommand.Create(() => Close?.Invoke(null));
        Browse = ReactiveCommand.Create(ExecuteBrowseAsync);
    }

    private async Task ExecuteBrowseAsync()
    {
        var folder = await _pathService.OpenFolderPickerAsync();
        if (!string.IsNullOrWhiteSpace(folder))
        {
            FilePath = folder;
        }
    }

    private void ExecuteConfirm()
    {
        if (InputIsNotValid())
        {
            _toastService.CreateAndShowErrorToast("Input must not contain illegal characters and cannot be empty.");
            return;
        }

        if (PageAlreadyExists())
        {
            _toastService.CreateAndShowErrorToast("A page with this name already exists. Please choose a different name.");
            return;
        }
        if (FolderNotSelected())
        {
            _toastService.CreateAndShowErrorToast("Please select a folder to save the document in.");
            return;
        }

        Close?.Invoke(System.IO.Path.Combine(FilePath!, DocumentName! + ".docx"));
    }

    private bool InputIsNotValid()
    {
        var illegalChars = System.IO.Path.GetInvalidFileNameChars();
        return string.IsNullOrWhiteSpace(DocumentName) ||
               DocumentName.Any(illegalChars.Contains);
    }
    private bool PageAlreadyExists()
    {
        return _pathService.GetExistingPageNames().Contains(DocumentName);
    }
    private bool FolderNotSelected()
    {
        return string.IsNullOrWhiteSpace(FilePath);
    }
}