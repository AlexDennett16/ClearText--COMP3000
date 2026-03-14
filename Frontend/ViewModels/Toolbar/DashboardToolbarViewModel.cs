using ClearText.BaseTypes.BaseViewModels;

namespace ClearText.ViewModels.Toolbar;

public class DashboardToolbarViewModel(ToolbarViewModel parent) : ViewModelBase
{
    public ToolbarViewModel Parent { get; } = parent;
}
