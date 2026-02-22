using System.Threading.Tasks;
using ClearText.DataObjects;

namespace ClearText.Interfaces;

public interface IGrammarService
{
    Task StartupAsync();
    Task<ClearTextResult?> CheckGrammarAsync(string text);
    void KillPythonProcess();
}