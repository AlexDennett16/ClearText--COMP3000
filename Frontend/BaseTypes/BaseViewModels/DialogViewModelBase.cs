using System;

namespace ClearText.BaseTypes.BaseViewModels
{
    public abstract class DialogViewModelBase<TResult> : ViewModelBase
    {
        public string? Title { get; init; }

        public Action<TResult?>? Close { get; set; }
    }
}