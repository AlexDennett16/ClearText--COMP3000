using System.Reactive;
using ClearText.BaseTypes.BaseViewModels;
using ClearText.Interfaces;
using ReactiveUI;

namespace ClearText.Dialogs;

public class StringDialogViewModel : DialogViewModelBase<string?>
{
  private string _stringValue;
  private readonly string _errorMessage;
  private readonly IToastService _toastService;

  public string stringValue
  {
    get => _stringValue;
    set => this.RaiseAndSetIfChanged(ref _stringValue, value);
  }

  public ReactiveCommand<Unit, Unit> Confirm { get; }
  public ReactiveCommand<Unit, Unit> Cancel { get; }

  public StringDialogViewModel(IToastService toastService,
    string errorMessage = "Please type something", string startingValue = "")
  {
    _toastService = toastService;
    _errorMessage = errorMessage;
    _stringValue = startingValue;

    Confirm = ReactiveCommand.Create(ExecuteConfirm);
    Cancel = ReactiveCommand.Create(() => Close?.Invoke(null));
  }

  private void ExecuteConfirm()
  {
    if (!string.IsNullOrWhiteSpace(stringValue))
    {
      Close?.Invoke(stringValue);
    }
    else
    {
      _toastService.CreateAndShowErrorToast(_errorMessage);
    }
  }
}