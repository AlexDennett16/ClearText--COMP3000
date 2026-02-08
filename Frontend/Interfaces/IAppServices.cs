namespace ClearText.Interfaces;

public interface IAppServices
{
  IToastService ToastService { get; }
  IDialogService DialogService { get; }
  IPathService PathService { get; }
  IGrammarService GrammarService { get; }
}