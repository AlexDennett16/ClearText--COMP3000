using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Threading;
using ClearText.BaseTypes;
using ClearText.BaseTypes.BaseViewModels;
using ClearText.Interfaces;
using ClearText.Toasts;

namespace ClearText.Services;

// Service responsible for managing the toasts shown in the application
// Limit displays to 5 at a time
public sealed class ToastService : BaseService, IToastService
{
    private const int MaxToastCount = 5;
    public ObservableCollection<ToastNotificationViewModelBase> Toasts { get; } = [];

    public void ShowToast(ToastNotificationViewModelBase toast)
    {
        Dispatcher.UIThread.Post(() =>
        {
            while (Toasts.Count > MaxToastCount)
            {
                Toasts.RemoveAt(0);
            }

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