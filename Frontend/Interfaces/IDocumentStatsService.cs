
using ClearText.DataObjects;

namespace ClearText.Interfaces;

public interface IDocumentStatsService
{
    DocumentStats GetDocumentStats(string text);
}