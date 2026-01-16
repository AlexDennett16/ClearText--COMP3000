using ClearText.Interfaces;


namespace ClearText.Services;

public class AppServices : IAppServices
{
    public IToastService ToastService { get; } = new ToastService();
    public IDialogService DialogService { get; } = new DialogService();
    public IPageStorageService PageStorageService { get; } = new PageStorageService();
}

