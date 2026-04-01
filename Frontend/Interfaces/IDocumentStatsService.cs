using ClearText.BaseTypes;
using ClearText.DataObjects;

namespace ClearText.Interfaces;

public interface IDocumentStatsService : IBaseServiceInterface
{
    DocumentStats GetDocumentStats(string text);
}