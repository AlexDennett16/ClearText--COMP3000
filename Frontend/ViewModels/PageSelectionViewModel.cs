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
    private readonly Action<string> _openEditor;
    private readonly IPathService _storage;

    private readonly IDialogService _dialogService;
    private readonly IToastService _toastService;

    private double _wrapWidth;

    public double WrapWidth
    {
        get => _wrapWidth;
        set => this.RaiseAndSetIfChanged(ref _wrapWidth, value);
    }

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

        var paths = _storage.LoadPageFilePaths();
        Pages = new ObservableCollection<PageViewModel>(
            paths.Select(p => new PageViewModel(p, _openEditor, RenamePage(p), DeletePage(p))));

        CreateNewDocumentCommand = ReactiveCommand.Create(CreateNewDocument);
    }


    private Action RenamePage(string filePath)
    {
        return () =>
        {
            _storage.ChangePageFilePath(filePath, filePath + "_renamed",
                Pages.Select(p => p.FilePath).ToList()); //Implement acc renaming functionality
            _toastService.CreateAndShowInfoToast(
                $"Document '{System.IO.Path.GetFileNameWithoutExtension(filePath)}' renamed.");
        };
    }

    private Action DeletePage(string filePath)
    {
        return () =>
        {
            Pages.Remove(Pages.First(p => p.FilePath == filePath));
            _storage.DeletePageFile(filePath, Pages.Select(p => p.FilePath).ToList());
            _toastService.CreateAndShowInfoToast(
                $"Document '{System.IO.Path.GetFileNameWithoutExtension(filePath)}' deleted.");
        };
    }

    private async void CreateNewDocument()
    {
        try
        {
            var dialog = new PageNameDialogViewModel(_toastService);
            var pageName = await _dialogService.ShowAsync(dialog);

            if (pageName is null)
                return;


            // 4. Build the new file path
            var newPath = _storage.CreatePageFilePath(pageName);

            // 5. Add to UI list
            Pages.Insert(0, new PageViewModel(newPath, _openEditor, RenamePage(newPath), DeletePage(newPath)));

            // 6. Persist
            _storage.SavePageFilePaths(Pages.Select(p => p.FilePath).ToList());

            _toastService.CreateAndShowInfoToast($"Document '{pageName}' created successfully.");

            // 7. Optionally open the editor
            //_openEditor(newPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error creating new document: " + ex.Message);
        }
    }
}