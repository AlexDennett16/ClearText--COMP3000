using System;
using System.Threading.Tasks;
using ClearText.BaseTypes;
using ClearText.BaseTypes.BaseViewModels;
using ClearText.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ClearText.Services;

// Service responsible for showing dialogs in the application. It uses an IUiHost to display the dialogs
public class DialogService(IUiHost host, IServiceProvider services) : BaseService, IDialogService
{
  private readonly IUiHost? _host = host;

  public Task<TResult?> ShowAsync<TResult>(DialogViewModelBase<TResult> dialog)
  {
    var tcs = new TaskCompletionSource<TResult?>();

    dialog.Close = result =>
    {
      if (_host != null) _host.DialogViewModel = null;
      tcs.SetResult(result);
    };

    if (_host != null) _host.DialogViewModel = dialog;

    return tcs.Task;
  }

  public Task<TResult?> ShowAsync<TDialog, TResult>() where TDialog : DialogViewModelBase<TResult>
  {
    var dialog = services.GetRequiredService<TDialog>();
    return ShowAsync(dialog);
  }

}