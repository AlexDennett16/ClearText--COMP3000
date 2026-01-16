using Avalonia.Controls;
using ClearText.ViewModels;

namespace ClearText.Views;

public partial class PageSelectionView : UserControl
{
    public PageSelectionView()
    {
        InitializeComponent();

        LayoutUpdated += (_, _) =>
        {
            if (DataContext is PageSelectionViewModel vm)
            {
                vm.WrapWidth = Bounds.Width;
            }
        };
    }
}