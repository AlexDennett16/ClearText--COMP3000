namespace ClearText.Interfaces;

public interface IAppServices
{
  IToastService ToastService { get; }
  IDialogService DialogService { get; }
  IPageStorageService PageStorageService { get; }
}