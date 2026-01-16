using System.Threading.Tasks;
using ClearText.BaseTypes.BaseViewModels;

namespace ClearText.Interfaces;

public interface IDialogService
{
  void SetHost(IDialogHost host);
  Task<TResult?> ShowAsync<TResult>(DialogViewModelBase<TResult> vm);
}