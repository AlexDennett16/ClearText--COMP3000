using System.Threading.Tasks;
using ClearText.BaseTypes;
using ClearText.BaseTypes.BaseViewModels;

namespace ClearText.Interfaces;

public interface IDialogService : IBaseServiceInterface
{
  Task<TResult?> ShowAsync<TResult>(DialogViewModelBase<TResult> dialog);

  Task<TResult?> ShowAsync<TDialog, TResult>() where TDialog : DialogViewModelBase<TResult>;

}