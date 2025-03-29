using System;
using System.Threading.Tasks;
using System.Windows.Input;
using ChatApplication.Services;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
namespace ChatApplication.ViewModels;

public class LoginViewModel : ViewModelBase
{
    private string _email = string.Empty;
    private string _password = string.Empty;
    private string _errorMessage = string.Empty;
    private readonly FirebaseAuthService _firebaseAuthService;
    private readonly AuthViewModel _authViewModel;

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
    public ICommand GoToRegisterCommand { get; }

    public LoginViewModel(AuthViewModel authViewModel)
    {
        _firebaseAuthService = App.ServiceProvider?.GetRequiredService<FirebaseAuthService>() ?? new FirebaseAuthService();
        _authViewModel = authViewModel;
        LoginCommand = ReactiveCommand.CreateFromTask(LoginAsync);
        GoToRegisterCommand = ReactiveCommand.Create(GoToRegister);
    }

    private void GoToRegister()
    {
        _authViewModel.ShowRegistrationView();
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

            // login
            var success = await _firebaseAuthService.LoginUserAsync(Email, Password);
            if (success.Success)
            {
                Console.WriteLine("logged in");
                // go to the main window
                _authViewModel.GoToMain();
            }
            else
            {
                // display the error
                ErrorMessage = success.Message;
            }
        }
        catch (Exception e)
        {
            _errorMessage = e.Message;
        }
    }
}