using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using ClearText.DialogFactories;
using ClearText.DialogFactoriesInterfaces;
using ClearText.Dialogs;
using ClearText.Enums;
using ClearText.Interfaces;
using ClearText.Services;
using ClearText.ViewModels;
using ClearText.ViewModels.Toolbar;
using Microsoft.Extensions.DependencyInjection;

namespace ClearText;

// ReSharper disable once PartialTypeWithSinglePart
public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;
    public override void Initialize()
    {
        Console.WriteLine("App.Initialize CALLED");
        AvaloniaXamlLoader.Load(this);
    }


    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var services = new ServiceCollection();

            // Windows and UI
            services.AddSingleton<MainWindow>();
            services.AddSingleton<IUiHost>(sp => sp.GetRequiredService<MainWindow>());
            services.AddSingleton<INavigationService, NavigationService>();

            // Services
            services.AddSingleton<IToastService, ToastService>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IPathService, PathService>();
            services.AddSingleton<IDocumentStatsService, DocumentStatsService>();
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<IFolderPickerService, FolderPickerService>();

            services.AddSingleton<IGrammarService, GrammarService>();
            // Register GrammarService as a Python startup task
            services.AddSingleton<IPythonStartupTask>(
                sp => (GrammarService)sp.GetRequiredService<IGrammarService>());


            // ViewModels
            services.AddTransient<PageSelectionViewModel>();
            services.AddTransient<TextEditorViewModel>();
            services.AddTransient<DashboardToolbarViewModel>();
            services.AddTransient<EditorToolbarViewModel>();

            //Dialogs
            services.AddTransient<SettingsDialogViewModel>();
            services.AddTransient<StringDialogViewModel>();
            services.AddTransient<DataDisplayDialogViewModel>();
            services.AddTransient<CreateNewDocumentDialogViewModel>();
            services.AddTransient<ConfirmCancelDialogViewModel>();
            services.AddTransient<ExitDocumentDialogViewModel>();


            // ViewModel Factories
            services.AddSingleton<Func<string, TextEditorViewModel>>(sp =>
                filePath => ActivatorUtilities.CreateInstance<TextEditorViewModel>(sp, filePath));

            services.AddSingleton<Func<PageSelectionViewModel>>(sp =>
                sp.GetRequiredService<PageSelectionViewModel>);

            services.AddSingleton<Func<DashboardToolbarViewModel>>(sp =>
                sp.GetRequiredService<DashboardToolbarViewModel>);

            services.AddSingleton<Func<string, EditorToolbarViewModel>>(sp =>
                filePath =>
                    ActivatorUtilities.CreateInstance<EditorToolbarViewModel>(
                        sp, filePath)
            );

            // Dialog Factories
            services.AddSingleton<ICreateNewDocumentDialogFactory, CreateNewDocumentDialogFactory>();
            services.AddSingleton<IStringDialogFactory, StringDialogFactory>();
            services.AddSingleton<IDataDisplayDialogFactory, DataDisplayDialogFactory>();
            services.AddSingleton<IConfirmCancelDialogFactory, ConfirmCancelDialogFactory>();
            services.AddSingleton<IExitDocumentDialogFactory, ExitDocumentDialogFactory>();
            Services = services.BuildServiceProvider();

            _ = StartBackgroundServicesAsync(Services);



            var window = Services.GetRequiredService<MainWindow>();

            var navigation = Services.GetRequiredService<INavigationService>();
            navigation.ShowDashboard();

            window.DataContext = window;
            desktop.MainWindow = window;
            desktop.Exit += OnAppExit;
        }

        base.OnFrameworkInitializationCompleted();
    }


    private static async Task StartBackgroundServicesAsync(IServiceProvider services)
    {
        foreach (var task in services.GetServices<IPythonStartupTask>())
        {
            await task.StartInBackground();
        }
    }

    public void ApplyTheme(AppTheme theme)
    {
        if (theme == AppTheme.Dark)
            RequestedThemeVariant = ThemeVariant.Dark;
        else
            RequestedThemeVariant = ThemeVariant.Light;
    }




    private static void OnAppExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        if (Services is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
