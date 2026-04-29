using System;
using ClearText.DialogFactoriesInterfaces;
using ClearText.Dialogs;
using Microsoft.Extensions.DependencyInjection;

namespace ClearText.DialogFactories;

public class CreateNewDocumentDialogFactory(IServiceProvider services)
        : ICreateNewDocumentDialogFactory
{
    public CreateNewDocumentDialogViewModel Create(string? previousFilePath = null)
    {
        return ActivatorUtilities.CreateInstance<CreateNewDocumentDialogViewModel>(
            services,
            previousFilePath ?? string.Empty);
    }
}