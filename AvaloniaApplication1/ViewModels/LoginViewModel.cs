using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Threading;
using AvaloniaApplication1.Views;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
namespace AvaloniaApplication1.ViewModels;

public class LoginViewModel : ViewModelBase
{
    private string _email = string.Empty;
    private string _password = string.Empty;
    private string _errorMessage = string.Empty;
    private Window _window;
    private readonly IServiceProvider _serviceProvider;

    public string Email
    {
        get => _email;
        set => this.RaiseAndSetIfChanged(ref _email, value);
    }
    
    public string Password
    {
        get => _password;
        set => this.RaiseAndSetIfChanged(ref _password, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }
    
    public ICommand LoginCommand { get; }

    public LoginViewModel(Window window)
    {
        _window = window;
        LoginCommand = ReactiveCommand.CreateFromTask(LoginAsync);
    }

    private async Task LoginAsync()
    {

        _errorMessage = string.Empty;

        if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
        {
            _errorMessage = "Email or password is empty";
            return;
        }

        try
        {
            Console.WriteLine("SUCCESS");
            
            // redirect to main window
            var mainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel()
            };
            
            mainWindow.Show();
            // close the login window
            _window.Close();
        }
        catch (Exception e)
        {
            _errorMessage = e.Message;
        }
    }
}