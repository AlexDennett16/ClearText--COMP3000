using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using ClearText.BaseTypes.BaseViewModels;
using ClearText.DialogFactoriesInterfaces;
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
    private readonly ICreateNewDocumentDialogFactory _createNewDocumentDialogFactory;
    private readonly IStringDialogFactory _stringDialogFactory;

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

    public PageSelectionViewModel(
        Action<string> openEditorCallback,
        IToastService toastService,
        IPathService pathService,
        IDialogService dialogService,
        ICreateNewDocumentDialogFactory createNewDocumentDialogFactory,
        IStringDialogFactory stringDialogFactory)
    {
        _toastService = toastService;
        _pathService = pathService;
        _dialogService = dialogService;
        _createNewDocumentDialogFactory = createNewDocumentDialogFactory;
        _stringDialogFactory = stringDialogFactory;
        _openEditor = openEditorCallback;

        RequestNewPageName = new Interaction<Unit, string?>();

        AllPages = new ObservableCollection<PageViewModel>(
            _pathService.PageFilePaths.Select(CreateVM));

        FilteredPages = new ObservableCollection<PageViewModel>(AllPages);


        _pathService.PagePathsChanged += RefreshPages;

        CreateNewDocumentCommand = ReactiveCommand.CreateFromTask(CreateNewDocument);
    }

    // ReSharper disable once InconsistentNaming
    private PageViewModel CreateVM(string path)
    {
        //Keep newing up, over DI, as this is just a UI element
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
            var dialog = _stringDialogFactory.Create(oldFileName);
            var newDocName = await _dialogService.ShowAsync(dialog);


            if (string.IsNullOrEmpty(newDocName) || newDocName == oldFileName)
                return;


            var directory = System.IO.Path.GetDirectoryName(oldPath);
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

    private async Task CreateNewDocument()
    {
        try
        {
            var dialog = _createNewDocumentDialogFactory.Create(
                _pathService.GetLastUsedFolderPath());

            var pageNameAndFilePath =
                await _dialogService.ShowAsync(dialog);

            if (string.IsNullOrWhiteSpace(pageNameAndFilePath))
                return;

            _pathService.AddPage(pageNameAndFilePath);

            _toastService.CreateAndShowInfoToast(
                $"Document '{System.IO.Path.GetFileNameWithoutExtension(pageNameAndFilePath)}' created.");
        }
        catch (Exception e)
        {
            _toastService.CreateAndShowErrorToast(
                "Error creating document: " + e.Message);
        }
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