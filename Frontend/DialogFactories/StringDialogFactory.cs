using System;
using ClearText.DialogFactoriesInterfaces;
using ClearText.Dialogs;
using Microsoft.Extensions.DependencyInjection;

namespace ClearText.DialogFactories;

public class StringDialogFactory(IServiceProvider services)
        : IStringDialogFactory
{
    public StringDialogViewModel Create(string? startingMessage = null)
    {
        return ActivatorUtilities.CreateInstance<StringDialogViewModel>(
            services,
            startingMessage ?? string.Empty);
    }
}