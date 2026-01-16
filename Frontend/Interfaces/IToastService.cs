using System;
using System.Collections.ObjectModel;
using ClearText.BaseTypes.BaseViewModels;

namespace ClearText.Interfaces;

public interface IToastService
{
  ObservableCollection<ToastNotificationViewModelBase> Toasts { get; }

  void CreateAndShowInfoToast(string message, TimeSpan? duration = null);
  void CreateAndShowErrorToast(string message, TimeSpan? duration = null);
}