using System;
using ClearText.DialogFactoriesInterfaces;
using ClearText.Dialogs;
using Microsoft.Extensions.DependencyInjection;

namespace ClearText.DialogFactories;

public class ConfirmCancelDialogFactory(IServiceProvider services)
        : IConfirmCancelDialogFactory
{
    public ConfirmCancelDialogViewModel Create(string? title = null, string? message = null)
    {
        return ActivatorUtilities.CreateInstance<ConfirmCancelDialogViewModel>(
            services,
            title ?? "Confirm Action",
            message ?? "Are you sure you want to proceed?"
        );
    }
}