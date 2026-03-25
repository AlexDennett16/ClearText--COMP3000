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
    private readonly IPathService _pathService;
    private readonly IDialogService _dialogService;
    private readonly IToastService _toastService;

    public ObservableCollection<PageViewModel> AllPages { get; }
    public ObservableCollection<PageViewModel> FilteredPages { get; private set; }

    public ReactiveCommand<Unit, Unit> CreateNewDocumentCommand { get; }
    public Interaction<Unit, string?> RequestNewPageName { get; }
    private string? _filterText;

    public string? FilterText
    {
        get => _filterText;
        set
        {
            this.RaiseAndSetIfChanged(ref _filterText, value);
            ApplyFilter();
        }
    }

    public PageSelectionViewModel(Action<string> openEditorCallback, IAppServices services)
    {
        _toastService = services.ToastService;
        _openEditor = openEditorCallback;
        _pathService = services.PathService;
        _dialogService = services.DialogService;

        RequestNewPageName = new Interaction<Unit, string?>();

        AllPages = new ObservableCollection<PageViewModel>(
            _pathService.PageFilePaths.Select(CreateVM));

        FilteredPages = new ObservableCollection<PageViewModel>(AllPages);


        _pathService.PagePathsChanged += RefreshPages;

        CreateNewDocumentCommand = ReactiveCommand.Create(CreateNewDocument);
    }

    // ReSharper disable once InconsistentNaming
    private PageViewModel CreateVM(string path)
    {
        return new PageViewModel(path, _openEditor, () => RenamePage(path), () => DeletePage(path));
    }

    private void RefreshPages()
    {
        AllPages.Clear();
        foreach (var p in _pathService.PageFilePaths)
            AllPages.Add(CreateVM(p));

        ApplyFilter();
    }

    private async void RenamePage(string oldPath)
    {
        try
        {
            var oldFileName = System.IO.Path.GetFileNameWithoutExtension(oldPath);
            var newDocName = await CallRenamePageDialogAsync(oldFileName);
            if (string.IsNullOrEmpty(newDocName) || newDocName == oldFileName)
                return;


            var directory = System.IO.Path.GetDirectoryName(oldPath)!;
            var extension = System.IO.Path.GetExtension(oldPath);

            var newPath = directory + "\\" + newDocName + extension;
            _pathService.RenamePage(oldPath, newPath);
            _toastService.CreateAndShowInfoToast("Document renamed to: " + newDocName);
            RefreshPages();
        }
        catch (Exception e)
        {
            _toastService.CreateAndShowErrorToast("Error renaming document: " + e.Message);
        }
    }

    private void DeletePage(string path)
    {
        _pathService.DeletePage(path);
        _toastService.CreateAndShowInfoToast("Document deleted.");
    }

    private async void CreateNewDocument()
    {
        try
        {
            var pageNameAndFilePath = await CallNewDocumentDialog();
            if (string.IsNullOrEmpty(pageNameAndFilePath))
                return;

            _pathService.AddPage(pageNameAndFilePath);

            _toastService.CreateAndShowInfoToast($"Document '{System.IO.Path.GetFileNameWithoutExtension(pageNameAndFilePath)}' created.");
        }
        catch (Exception e)
        {
            _toastService.CreateAndShowErrorToast("Error creating document: " + e.Message);
        }
    }

    private async Task<string?> CallNewDocumentDialog()
    {
        var dialog = new CreateNewDocumentDialogViewModel(
            _toastService,
            _pathService,
            previousFilePath: _pathService.GetLastUsedFolderPath());
        var result = await _dialogService.ShowAsync(dialog);
        return result;
    }
    private async Task<string> CallRenamePageDialogAsync(string startingValue = "")
    {
        var dialog = new StringDialogViewModel(_toastService, _pathService, startingValue);
        var result = await _dialogService.ShowAsync(dialog);
        return result ?? string.Empty;
    }

    private void ApplyFilter()
    {
        if (string.IsNullOrWhiteSpace(FilterText))
        {
            FilteredPages = new ObservableCollection<PageViewModel>(AllPages);
            this.RaisePropertyChanged(nameof(FilteredPages));
            return;
        }

        var filtered = AllPages
            .Where(p => p.Title.Contains(FilterText, StringComparison.OrdinalIgnoreCase))
            .ToList();

        FilteredPages = new ObservableCollection<PageViewModel>(filtered);
        this.RaisePropertyChanged(nameof(FilteredPages));
    }
}