using System;
using System.Reactive;
using ReactiveUI;

namespace ClearText.BaseTypes.BaseViewModels
{
    public abstract class DialogViewModelBase<TResult> : ViewModelBase
    {
        public string? Title { get; init; }
        public string? Message { get; set; }
        public Action<TResult?>? Close { get; set; }
        public ReactiveCommand<Unit, Unit>? CloseCommand { get; set; }
        public ReactiveCommand<Unit, Unit>? ConfirmCommand { get; set; }
    }
}