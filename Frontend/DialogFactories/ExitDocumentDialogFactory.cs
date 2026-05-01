using System;
using ClearText.DialogFactoriesInterfaces;
using ClearText.Dialogs;
using Microsoft.Extensions.DependencyInjection;

namespace ClearText.DialogFactories;

public class ExitDocumentDialogFactory(IServiceProvider services)
        : IExitDocumentDialogFactory
{
    public ExitDocumentDialogViewModel Create(string? title = null, string? message = null)
    {
        return ActivatorUtilities.CreateInstance<ExitDocumentDialogViewModel>(
            services,
            title ?? string.Empty,
            message ?? string.Empty);
    }
}