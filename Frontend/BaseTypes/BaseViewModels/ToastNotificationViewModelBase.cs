using System;

namespace ClearText.BaseTypes.BaseViewModels;

public abstract class ToastNotificationViewModelBase(string message, TimeSpan? duration = null)
{
    public string Message { get; } = message;
    public TimeSpan Duration { get; } = duration ?? TimeSpan.FromSeconds(5);
}