using System.Threading.Tasks;
using ClearText.BaseTypes;
using ClearText.BaseTypes.BaseViewModels;
using ClearText.Interfaces;

namespace ClearText.Services;

public class DialogService(IUiHost host) : BaseService, IDialogService
{
  private readonly IUiHost? _host = host;

  public Task<TResult?> ShowAsync<TResult>(DialogViewModelBase<TResult> vm)
  {
    var tcs = new TaskCompletionSource<TResult?>();

    vm.Close = result =>
    {
      if (_host != null) _host.DialogViewModel = null;
      tcs.SetResult(result);
    };

    if (_host != null) _host.DialogViewModel = vm;

    return tcs.Task;
  }
}