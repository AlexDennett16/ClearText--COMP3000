using ClearText.BaseTypes;

namespace ClearText.Interfaces;

public interface IAppServices : IBaseServiceInterface
{
  IToastService ToastService { get; }
  IDialogService DialogService { get; }
  IPathService PathService { get; }
  IGrammarService GrammarService { get; }
  IDocumentStatsService DocumentStatsService { get; }
}