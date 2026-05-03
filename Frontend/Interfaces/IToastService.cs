using System;

namespace ClearText.Interfaces;

public interface IToastService
{
  void CreateAndShowInfoToast(string message, TimeSpan? duration = null);
  void CreateAndShowErrorToast(string message, TimeSpan? duration = null);
}