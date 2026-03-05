using System.Linq;
using System.Reactive;
using ClearText.BaseTypes.BaseViewModels;
using ClearText.Interfaces;
using ReactiveUI;

namespace ClearText.Dialogs;

public class StringDialogViewModel : DialogViewModelBase<string?>
{
  private string _stringValue;
  private readonly IToastService _toastService;
  private readonly IPathService _pathService;

  public string StringValue
  {
    get => _stringValue;
    set => this.RaiseAndSetIfChanged(ref _stringValue, value);
  }

  public ReactiveCommand<Unit, Unit> Confirm { get; }
  public ReactiveCommand<Unit, Unit> Cancel { get; }

  public StringDialogViewModel(IToastService toastService, IPathService pathService, string startingValue = "")
  {
    _toastService = toastService;
    _pathService = pathService;
    _stringValue = startingValue;

    Title = "Enter a value";

    Confirm = ReactiveCommand.Create(ExecuteConfirm);
    Cancel = ReactiveCommand.Create(() => Close?.Invoke(null));
  }

  private void ExecuteConfirm()
  {
    if (InputIsNotValid())
    {
      _toastService.CreateAndShowErrorToast("Input must not contain illegal characters and cannot be empty.");
    }
    else if (_pathService.GetExistingPageNames().Contains(StringValue))
    {
      _toastService.CreateAndShowErrorToast("A page with this name already exists. Please choose a different name.");
    }
    else
    {
      Close?.Invoke(StringValue);
    }
  }

  private bool InputIsNotValid()
  {
    var illegalChars = System.IO.Path.GetInvalidFileNameChars();
    return string.IsNullOrWhiteSpace(StringValue) ||
           StringValue.Any(illegalChars.Contains); //TODO switch statement this?
  }
}