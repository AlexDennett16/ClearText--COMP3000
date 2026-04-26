using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using ClearText.Enums;
using ClearText.Services;

namespace ClearText;

public partial class App : Application
{
    public static AppServices Services { get; private set; } = null!;
    public static Window? MainWindow { get; private set; }

    public override void Initialize()
    {
        Console.WriteLine("App.Initialize CALLED");
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var window = new MainWindow();
            MainWindow = window;

            Services = new AppServices(window);

            window.DataContext = new ViewModels.MainWindowViewModel(Services);

            ApplyTheme(Services.SettingsService.CurrentTheme);

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
        Services.Dispose();
    }
}
