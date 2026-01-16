using System.Reactive;
using ClearText.BaseTypes.BaseViewModels;
using ClearText.Interfaces;
using ReactiveUI;

namespace ClearText.Dialogs;

public class PageNameDialogViewModel : DialogViewModelBase<string?>
{
  private string _pageName = "";
  private readonly IToastService _toastService;

  public string PageName
  {
    get => _pageName;
    set => this.RaiseAndSetIfChanged(ref _pageName, value);
  }

  public ReactiveCommand<Unit, Unit> Confirm { get; }
  public ReactiveCommand<Unit, Unit> Cancel { get; }

  public PageNameDialogViewModel(IToastService toastService)
  {
    _toastService = toastService;

    Confirm = ReactiveCommand.Create(ExecuteConfirm);
    Cancel = ReactiveCommand.Create(() => Close?.Invoke(null));
  }

  private void ExecuteConfirm()
  {
    if (!string.IsNullOrWhiteSpace(PageName))
    {
      Close?.Invoke(PageName);
    }

    _toastService.CreateAndShowErrorToast("Document name required.");
  }
}