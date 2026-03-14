using ClearText.BaseTypes.BaseViewModels;

namespace ClearText.ViewModels.Toolbar;

public class EditorToolbarViewModel(ToolbarViewModel parent) : ViewModelBase
{
    public ToolbarViewModel Parent { get; } = parent;
}