using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
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
            Services = new AppServices();

            var window = new MainWindow();
            desktop.MainWindow = window;
            MainWindow = window;

            // Attach the dialog host to the dialog service
            Services.DialogService.SetHost(window);
        }

        base.OnFrameworkInitializationCompleted();
    }
}