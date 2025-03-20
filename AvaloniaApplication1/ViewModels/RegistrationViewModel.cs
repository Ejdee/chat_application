using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using AvaloniaApplication1.Services;
using AvaloniaApplication1.Views;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;

namespace AvaloniaApplication1.ViewModels;

public class RegistrationViewModel : ViewModelBase
{
    private string _email = string.Empty;
    private string _username = string.Empty;
    private string _password = string.Empty;
    private string _errorMessage = string.Empty;
    private readonly FirebaseAuthService _firebaseAuthService;
    private readonly AuthViewModel _authViewModel;

    public string Email
    {
        get => _email;
        set => this.RaiseAndSetIfChanged(ref _email, value);
    }
    public string Username
    {
        get => _username;
        set => this.RaiseAndSetIfChanged(ref _username, value);
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
    
    public ICommand RegisterCommand { get; }
    public ICommand GoToLoginCommand { get; }

    public RegistrationViewModel(AuthViewModel authViewModel)
    {
        _firebaseAuthService = App.ServiceProvider?.GetService<FirebaseAuthService>() ?? new FirebaseAuthService();
        RegisterCommand = ReactiveCommand.CreateFromTask(RegisterAsync);
        GoToLoginCommand = ReactiveCommand.Create(GoToLogin);
        _authViewModel = authViewModel;
    }

    private void GoToLogin()
    {
        _authViewModel.ShowLoginView();     
    }

    private async Task RegisterAsync()
    {
        if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
        {
            ErrorMessage = "Please fill all the fields";
            return;
        }

        try
        {
            var success = await _firebaseAuthService.RegisterUserAsync(Email, Password, Username);
            if (success != null)
            {
                _authViewModel.GoToMain();
            }

        }
        catch (Exception e)
        {
            ErrorMessage = e.Message;
        }
    }
}