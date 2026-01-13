using System.Reactive;
using ClearText.BaseTypes.BaseViewModels;
using ReactiveUI;

namespace ClearText.Dialogs;

public class PageNameDialogViewModel : DialogViewModelBase<string?>
{
  public string PageName { get; set; } = "";

  public ReactiveCommand<Unit, Unit> Confirm { get; }
  public ReactiveCommand<Unit, Unit> Cancel { get; }

  public PageNameDialogViewModel()
  {
    Confirm = ReactiveCommand.Create(() => Close?.Invoke(PageName));
    Cancel = ReactiveCommand.Create(() => Close?.Invoke(null));
  }
}