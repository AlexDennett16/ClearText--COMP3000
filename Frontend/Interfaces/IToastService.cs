using System;
using System.Collections.ObjectModel;
using ClearText.BaseTypes;
using ClearText.BaseTypes.BaseViewModels;

namespace ClearText.Interfaces;

public interface IToastService : IBaseServiceInterface
{
  ObservableCollection<ToastNotificationViewModelBase> Toasts { get; }

  void CreateAndShowInfoToast(string message, TimeSpan? duration = null);
  void CreateAndShowErrorToast(string message, TimeSpan? duration = null);
}