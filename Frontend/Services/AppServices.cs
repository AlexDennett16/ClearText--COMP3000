using ClearText.Interfaces;


namespace ClearText.Services;

public class AppServices : IAppServices
{
    public IToastService ToastService { get; } = new ToastService();
    public IDialogService DialogService { get; } = new DialogService();
    public IPathService PathService { get; }
    public IGrammarService GrammarService { get; }

    public AppServices()
    {
        PathService = new PathService();
        GrammarService = new GrammarService(PathService);
    }
}