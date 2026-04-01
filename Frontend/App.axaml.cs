using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AvaloniaEdit.Editing;
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

            desktop.MainWindow = window;

            desktop.Exit += OnAppExit;
        }

        base.OnFrameworkInitializationCompleted();

        Services.GrammarService.StartPythonServer();
    }



    private void OnAppExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        Services.Dispose();
    }


}
