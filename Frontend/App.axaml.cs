using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
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

            // Services
            services.AddSingleton<IToastService, ToastService>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IPathService, PathService>();
            services.AddSingleton<IGrammarService, GrammarService>();
            services.AddSingleton<IDocumentStatsService, DocumentStatsService>();
            services.AddSingleton<ISettingsService, SettingsService>();

            // ViewModels
            services.AddSingleton<MainWindowViewModel>();
            services.AddTransient<PageSelectionViewModel>();
            services.AddTransient<TextEditorViewModel>();
            services.AddTransient<DashboardToolbarViewModel>();
            services.AddTransient<EditorToolbarViewModel>();

            // Factories
            services.AddSingleton<Func<string, Action, TextEditorViewModel>>(sp =>
                (filePath, close) =>
                    ActivatorUtilities.CreateInstance<TextEditorViewModel>(
                        sp, filePath, close)
            );
            services.AddSingleton<Func<Action<string>, PageSelectionViewModel>>(sp =>
                openEditorCallback =>
                    ActivatorUtilities.CreateInstance<PageSelectionViewModel>(
                        sp, openEditorCallback)
            );

            services.AddSingleton<Func<DashboardToolbarViewModel>>(sp =>
                sp.GetRequiredService<DashboardToolbarViewModel>);

            services.AddSingleton<Func<string, EditorToolbarViewModel>>(sp =>
                (filePath) =>
                    ActivatorUtilities.CreateInstance<EditorToolbarViewModel>(
                        sp, filePath)
            );

            Services = services.BuildServiceProvider();

            var window = Services.GetRequiredService<MainWindow>();
            window.DataContext = Services.GetRequiredService<MainWindowViewModel>();

            desktop.MainWindow = window;
            desktop.Exit += OnAppExit;
        }

        base.OnFrameworkInitializationCompleted();
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
