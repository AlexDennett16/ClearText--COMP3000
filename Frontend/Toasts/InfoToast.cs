using System;
using ClearText.BaseTypes.BaseViewModels;

namespace ClearText.Toasts;

public class InfoToast(string message, TimeSpan? duration = null) : ToastNotificationViewModelBase(message, duration);