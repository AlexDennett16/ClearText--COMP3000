using ClearText.BaseTypes;
using ClearText.Interfaces;


namespace ClearText.Services;

public class AppServices(IUiHost host) : BaseService, IAppServices
{
    public IToastService ToastService { get; } = new ToastService();
    public IDialogService DialogService { get; } = new DialogService(host);
    public IPathService PathService { get; } = new PathService(host.Window);
    public IGrammarService GrammarService { get; } = new GrammarService();
    public IDocumentStatsService DocumentStatsService { get; } = new DocumentStatsService();
    public ISettingsService SettingsService { get; } = new SettingsService();

    public override void Dispose()
    {
        ToastService.Dispose();
        DialogService.Dispose();
        PathService.Dispose();
        GrammarService.Dispose();
        DocumentStatsService.Dispose();
        base.Dispose();
    }
}