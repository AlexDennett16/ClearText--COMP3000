using System;
using ReactiveUI;

namespace ClearText.BaseTypes.BaseViewModels;

public class ViewModelBase : ReactiveObject, IDisposable
{
    private bool _disposed;
    public void Dispose()
    {
        if (_disposed) return;

        Dispose(true);
        GC.SuppressFinalize(this);
        _disposed = true;
    }

    protected virtual void Dispose(bool disposing)
    {
        // Override in derived classes
    }
};

