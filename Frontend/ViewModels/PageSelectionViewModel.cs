using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using ClearText.BaseTypes.BaseViewModels;
using ClearText.Dialogs;
using ClearText.Services;
using ReactiveUI;

namespace ClearText.ViewModels;

public class PageSelectionViewModel : ViewModelBase
{
    private readonly Action<string> _openEditor;
    private readonly PageStorageService _storage;

    private readonly DialogService _dialogService;

    private double _wrapWidth;

    public double WrapWidth
    {
        get => _wrapWidth;
        set => this.RaiseAndSetIfChanged(ref _wrapWidth, value);
    }

    public ObservableCollection<PageViewModel> Pages { get; }

    public ReactiveCommand<Unit, Unit> CreateNewDocumentCommand { get; }

    public Interaction<Unit, string?> RequestNewPageName { get; }


    public PageSelectionViewModel(Action<string> openEditorCallback, PageStorageService storage,
        DialogService dialogService)
    {
        _openEditor = openEditorCallback;
        _storage = storage;
        _dialogService = dialogService;

        RequestNewPageName = new Interaction<Unit, string?>();

        var paths = storage.LoadFilePaths();
        Pages = new ObservableCollection<PageViewModel>(
            paths.Select(p => new PageViewModel(p, openEditorCallback)));

        CreateNewDocumentCommand = ReactiveCommand.Create(CreateNewDocument);
    }

    private async void CreateNewDocument()
    {
        try
        {
            var dialog = new PageNameDialogViewModel();
            var pageName = await _dialogService.ShowAsync(dialog);

            if (pageName is null)
                return;


            // 4. Build the new file path
            var newPath = _storage.CreateFilePath(pageName);

            // 5. Add to UI list
            Pages.Add(new PageViewModel(newPath, _openEditor));

            // 6. Persist
            _storage.SaveFilePaths(Pages.Select(p => p.FilePath).ToList());

            // 7. Optionally open the editor
            //_openEditor(newPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error creating new document: " + ex.Message);
        }
    }
}