using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using ClearText.BaseTypes.BaseViewModels;
using ClearText.Dialogs;
using ClearText.Interfaces;
using ReactiveUI;

namespace ClearText.ViewModels;

public class PageSelectionViewModel : ViewModelBase
{
    private double _wrapWidth;

    public double WrapWidth
    {
        get => _wrapWidth;
        set => this.RaiseAndSetIfChanged(ref _wrapWidth, value);
    }

    private readonly Action<string> _openEditor;
    private readonly IPathService _storage;
    private readonly IDialogService _dialogService;
    private readonly IToastService _toastService;

    public ObservableCollection<PageViewModel> Pages { get; }

    public ReactiveCommand<Unit, Unit> CreateNewDocumentCommand { get; }
    public Interaction<Unit, string?> RequestNewPageName { get; }

    public PageSelectionViewModel(Action<string> openEditorCallback, IAppServices services)
    {
        _toastService = services.ToastService;
        _openEditor = openEditorCallback;
        _storage = services.PathService;
        _dialogService = services.DialogService;

        RequestNewPageName = new Interaction<Unit, string?>();

        Pages = new ObservableCollection<PageViewModel>(
            _storage.PageFilePaths.Select(CreateVM));

        _storage.PagePathsChanged += RefreshPages;

        CreateNewDocumentCommand = ReactiveCommand.Create(CreateNewDocument);
    }

    private PageViewModel CreateVM(string path)
    {
        return new PageViewModel(path, _openEditor, () => RenamePage(path), () => DeletePage(path));
    }

    private void RefreshPages()
    {
        Pages.Clear();
        foreach (var p in _storage.PageFilePaths)
            Pages.Add(CreateVM(p));
    }

    private async void RenamePage(string oldPath)
    {
        try
        {
            var oldFileName = System.IO.Path.GetFileNameWithoutExtension(oldPath);
            var newDocName = await CallDialog(oldFileName);
            if (string.IsNullOrEmpty(newDocName) || newDocName == oldFileName)
                return;


            var directory = System.IO.Path.GetDirectoryName(oldPath)!;
            var extension = System.IO.Path.GetExtension(oldPath);

            var newPath = directory + "\\" + newDocName + extension;
            _storage.RenamePage(oldPath, newPath);
            _toastService.CreateAndShowInfoToast("Document renamed to:" + newDocName);
            RefreshPages();
        }
        catch (Exception e)
        {
            _toastService.CreateAndShowErrorToast("Error renaming document: " + e.Message);
        }
    }

    private void DeletePage(string path)
    {
        _storage.DeletePage(path);
        _toastService.CreateAndShowInfoToast("Document deleted.");
    }

    private async void CreateNewDocument()
    {
        try
        {
            var pageName = await CallDialog();
            if (string.IsNullOrEmpty(pageName))
                return;

            var newPath = _storage.CreatePageFilePath(pageName);
            _storage.AddPage(newPath);

            _toastService.CreateAndShowInfoToast($"Document '{pageName}' created.");
        }
        catch (Exception e)
        {
            _toastService.CreateAndShowErrorToast("Error creating document: " + e.Message);
        }
    }

    private async Task<string> CallDialog(string startingValue = "")
    {
        var dialog = new StringDialogViewModel(_toastService, _storage, startingValue: startingValue);
        var result = await _dialogService.ShowAsync(dialog);
        return result ?? string.Empty;
    }
}