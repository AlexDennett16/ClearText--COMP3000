using System;
using System.Collections.ObjectModel;
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

    public PageSelectionViewModel(Action<string> OpenEditor)
    {
        // Populate for testing
        for (int i = 1; i <= 50; i++)
        {
            Pages.Add(new PageViewModel(OpenEditor));
        }
    }
}
