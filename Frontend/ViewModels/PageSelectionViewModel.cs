using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
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

    private void RenamePage(string oldPath)
    {
        var directory = System.IO.Path.GetDirectoryName(oldPath)!;
        var nameWithoutExt =
            System.IO.Path.GetFileNameWithoutExtension(oldPath) + "_renamed"; // TODO real rename dialog
        var extension = System.IO.Path.GetExtension(oldPath);

        var newPath = directory + "\\" + nameWithoutExt + extension;
        _storage.RenamePage(oldPath, newPath);
        _toastService.CreateAndShowInfoToast("Document renamed.");
        RefreshPages();
    }

    private void DeletePage(string path)
    {
        _storage.DeletePage(path);
        _toastService.CreateAndShowInfoToast("Document deleted.");
    }

    private async void CreateNewDocument()
    {
        var dialog = new PageNameDialogViewModel(_toastService);
        var pageName = await _dialogService.ShowAsync(dialog);

        if (pageName is null)
            return;

        var newPath = _storage.CreatePageFilePath(pageName);
        _storage.AddPage(newPath);

        _toastService.CreateAndShowInfoToast($"Document '{pageName}' created.");
    }
}