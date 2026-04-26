using System;
using ReactiveUI;

namespace ClearText.BaseTypes.BaseViewModels;

public class ViewModelBase : ReactiveObject, IDisposable
{
    public virtual void Dispose()
    {
        // Override in derived classes if needed
    }
};

