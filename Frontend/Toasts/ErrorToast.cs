using System;
using ClearText.BaseTypes.BaseViewModels;

namespace ClearText.Toasts;

public class ErrorToast(string message, TimeSpan? duration = null) : ToastNotificationViewModelBase(message, duration);