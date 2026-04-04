using System.Threading.Tasks;
using ClearText.BaseTypes;
using ClearText.DataObjects;

namespace ClearText.Interfaces;

public interface IGrammarService : IBaseServiceInterface
{
    Task<ClearTextResult?> CheckGrammarAsync(string text);
}