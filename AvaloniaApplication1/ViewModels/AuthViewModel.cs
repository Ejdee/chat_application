using Avalonia.Controls;
using AvaloniaApplication1.Views;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;

namespace AvaloniaApplication1.ViewModels;

public class AuthViewModel : ViewModelBase
{
    private ViewModelBase _currentViewModel;
    private LoginViewModel _loginViewModel;
    private RegistrationViewModel _registrationViewModel;
    private Window _window;

    public ViewModelBase CurrentViewModel
    {
        get => _currentViewModel;
        set => this.RaiseAndSetIfChanged(ref _currentViewModel, value);
    }

    public AuthViewModel(Window window)
    {
        _window = window;
        _loginViewModel = new LoginViewModel(this);
        _registrationViewModel = new RegistrationViewModel(this);
        CurrentViewModel = _loginViewModel;
    }

    public void ShowLoginView() => CurrentViewModel = _loginViewModel; 
    public void ShowRegistrationView() => CurrentViewModel = _registrationViewModel;
    
    public void GoToMain()
    {
        var mainWindow = new MainWindow
        {
            DataContext = App.ServiceProvider?.GetRequiredService<MainWindowViewModel>() ?? new MainWindowViewModel()
        };
        mainWindow.Show();
        _window.Close();
    }
}