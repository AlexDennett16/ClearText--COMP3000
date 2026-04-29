using System.Threading.Tasks;

namespace ClearText.Interfaces;

public interface IPythonStartupTask
{
    Task StartInBackground();
}