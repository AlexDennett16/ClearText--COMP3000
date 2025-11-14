using System.Collections.ObjectModel;
using ClearText.BaseViewModels;
using ReactiveUI;

namespace ClearText.ViewModels;

public class PageSelectionMainViewModel : ViewModelBase
{
    public ObservableCollection<PageSelectionViewModel> Pages { get; } = new();

    public PageSelectionMainViewModel()
    {
        // Populate for testing
        for (int i = 1; i <= 50; i++)
        {
            Pages.Add(new PageSelectionViewModel());
        }
    }
}
