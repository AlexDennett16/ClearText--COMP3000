using System;
using ClearText.BaseTypes;

namespace ClearText.Interfaces;

public interface IToastService : IBaseServiceInterface
{
  void CreateAndShowInfoToast(string message, TimeSpan? duration = null);
  void CreateAndShowErrorToast(string message, TimeSpan? duration = null);
}