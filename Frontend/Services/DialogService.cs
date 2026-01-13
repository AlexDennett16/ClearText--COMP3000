using System.Threading.Tasks;
using ClearText.BaseTypes.BaseViewModels;
using ClearText.ViewModels;

namespace ClearText.Services;

public class DialogService(MainWindowViewModel host)
{
    public Task<TResult?> ShowAsync<TResult>(DialogViewModelBase<TResult> vm)
  {
    var tcs = new TaskCompletionSource<TResult?>();

    vm.Close = result =>
    {
      host.DialogViewModel = null;
      tcs.SetResult(result);
    };

    host.DialogViewModel = vm;

    return tcs.Task;
  }
}