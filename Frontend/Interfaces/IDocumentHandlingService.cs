using System.Threading.Tasks;

namespace ClearText.Interfaces;

public interface IDocumentHandlingService
{
    string LoadText(string filePath);
    Task SaveTextAsync(string filePath, string documentText);
}