using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ClearText.BaseTypes.BaseViewModels;
using ClearText.Toasts;
using ReactiveUI;

namespace ClearText.Services;

public class ToastService : ReactiveObject
{
    public ObservableCollection<ToastNotificationViewModelBase> Toasts { get; } = [];

    public void ShowToast(ToastNotificationViewModelBase toast)
    {
        Toasts.Add(toast);

        Task.Delay(toast.Duration).ContinueWith(_ =>
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() => { Toasts.Remove(toast); });
        });
    }

    //Do this to create toasts, can expand to be errors/warnings later
    public void CreateAndShowInfoToast(string message, TimeSpan? duration = null)
    {
        ShowToast(new InfoToast(message, duration));
    }
}