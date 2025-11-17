using Avalonia;
using Avalonia.Controls;
using ClearText.ViewModels;

namespace ClearText.Views;

public partial class PageSelectionView : UserControl
{
    private PageSelectionViewModel _viewModel;

    public PageSelectionView()
    {
        InitializeComponent();
        _viewModel = new PageSelectionViewModel();
        DataContext = _viewModel;


        LayoutUpdated += (_, _) =>
        {
            _viewModel.WrapWidth = Bounds.Width;
            _viewModel.PageWidth = Bounds.Width / 5;
            _viewModel.PageHeight = _viewModel.PageWidth * 1.61;
        };
    }
}