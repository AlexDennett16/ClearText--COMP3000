using System.Reactive;
using ClearText.BaseTypes.BaseViewModels;
using ClearText.DataObjects;
using ReactiveUI;

namespace ClearText.Dialogs;

public class DataDisplayDialogViewModel : DialogViewModelBase<string>
{
    public DocumentStats Stats { get; set; }
    public ReactiveCommand<Unit, Unit> CloseCommand { get; }

    // We would make this a generic data display then create a new class onto which would accept stats, but for now this is the only usage 
    public DataDisplayDialogViewModel(
        DocumentStats stats,
        string title = "Data Display")
    {
        Title = title;
        Stats = stats;

        CloseCommand = ReactiveCommand.Create(() => Close?.Invoke(null));
    }
}