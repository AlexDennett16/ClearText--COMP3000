using System;
using ClearText.DataObjects;
using ClearText.DialogFactoriesInterfaces;
using ClearText.Dialogs;
using Microsoft.Extensions.DependencyInjection;

namespace ClearText.DialogFactories;

public class DataDisplayDialogFactory(IServiceProvider services)
        : IDataDisplayDialogFactory
{
    public DataDisplayDialogViewModel Create(DocumentStats stats, string? title = null)
    {
        return ActivatorUtilities.CreateInstance<DataDisplayDialogViewModel>(
            services,
            stats,
            title ?? "Data Display");
    }
}