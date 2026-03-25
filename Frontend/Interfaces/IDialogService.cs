using System.Threading.Tasks;
using ClearText.BaseTypes.BaseViewModels;

namespace ClearText.Interfaces;

public interface IDialogService
{
  Task<TResult?> ShowAsync<TResult>(DialogViewModelBase<TResult> vm);
}