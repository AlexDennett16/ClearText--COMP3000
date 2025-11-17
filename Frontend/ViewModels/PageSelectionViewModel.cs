using System.Collections.ObjectModel;
using ClearText.BaseViewModels;
using ReactiveUI;

namespace ClearText.ViewModels;

public class PageSelectionViewModel : ViewModelBase
{
    private double _wrapWidth;
    private double _pageWidth;
    private double _pageHeight;
    public double WrapWidth
    {
        get => _wrapWidth;
        set => this.RaiseAndSetIfChanged(ref _wrapWidth, value);
    }

    public double PageWidth
    {
        get => _pageWidth;
        set => this.RaiseAndSetIfChanged(ref _pageWidth, value);
    }
    public double PageHeight
    {
        get => _pageHeight;
        set => this.RaiseAndSetIfChanged(ref _pageHeight, value);
    }

    public ObservableCollection<PageViewModel> Pages { get; } = new();

    public PageSelectionViewModel()
    {
        // Populate for testing
        for (int i = 1; i <= 50; i++)
        {
            Pages.Add(new PageViewModel());
        }
    }
}
