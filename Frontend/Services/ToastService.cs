using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Threading;
using ClearText.BaseTypes.BaseViewModels;
using ClearText.Interfaces;
using ClearText.Toasts;
using ReactiveUI;

namespace ClearText.Services;

public class ToastService : ReactiveObject, IToastService
{
    public ObservableCollection<ToastNotificationViewModelBase> Toasts { get; } = [];

    public void ShowToast(ToastNotificationViewModelBase toast)
    {
        Dispatcher.UIThread.Post(() =>
        {
            Toasts.Add(toast);

            Task.Delay(toast.Duration).ContinueWith(_ => { Dispatcher.UIThread.Post(() => Toasts.Remove(toast)); });
        });
    }

    //Do this to create toasts, can expand to be errors/warnings later
    public void CreateAndShowInfoToast(string message, TimeSpan? duration = null)
    {
        ShowToast(new InfoToast(message, duration));
    }

    public void CreateAndShowErrorToast(string message, TimeSpan? duration = null)
    {
        ShowToast(new ErrorToast(message, duration));
    }
}