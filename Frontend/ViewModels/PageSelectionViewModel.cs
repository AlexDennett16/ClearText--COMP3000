using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ClearText.BaseViewModels;
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

    public ObservableCollection<PageViewModel> Pages { get; } = new();

    public PageSelectionViewModel(Action<string> openEditorCallback, PageStorageService storage)
    {
        var paths = storage.LoadFilePaths();
        Pages = new ObservableCollection<PageViewModel>(
                    paths.Select(p => new PageViewModel(p, openEditorCallback)));
    }
}
