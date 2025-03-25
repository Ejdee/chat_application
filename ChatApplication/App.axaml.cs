using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ChatApplication.Services;
using ChatApplication.ViewModels;
using ChatApplication.Views;
using Microsoft.Extensions.DependencyInjection;

namespace ChatApplication;

public partial class App : Application
{
    public static IServiceProvider? ServiceProvider { get; private set; }
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            DisableAvaloniaDataAnnotationValidation();

            var services = new ServiceCollection();

            services.AddSingleton<FirebaseAuthService>();
            services.AddSingleton<FirebaseService>();

            services.AddTransient<LoginViewModel>();
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<UsersFieldViewModel>();
            services.AddTransient<ChatViewModel>();
            
            ServiceProvider = services.BuildServiceProvider(); 
            
            var authView = new AuthView();
            desktop.MainWindow = authView;
            authView.DataContext = new AuthViewModel(authView);
            
    //        desktop.Exit += OnExit;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }

    //private async Task OnExitAsync()
    //{
    //    var firebaseService = ServiceProvider?.GetService<FirebaseService>();
    //    if (firebaseService != null)
    //    {
    //        try
    //        {
    //            Console.WriteLine("logging out user");
    //            await firebaseService.LogOutUserAsync();
    //            Console.WriteLine("Logged out");
    //            
    //            Console.WriteLine("Clearing the currently reading book");
    //            await firebaseService.ClearTheCurrentlyReadingAsync();
    //            Console.WriteLine("Clearing the currently reading book done");
    //        }
    //        catch (Exception e)
    //        {
    //            Console.WriteLine("Error on exiting the program: " + e);
    //        }
    //    }
    //}
    //
    //private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    //{
    //    Task.Run(async () =>
    //    {
    //        await OnExitAsync();
    //    });
    //}
}