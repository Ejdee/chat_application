using System;
using System.Threading.Tasks;
using ReactiveUI;

namespace ChatApplication.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private ChatViewModel? _currentChat;
    public UsersFieldViewModel UsersField { get; }
    
    public ChatViewModel? CurrentChat
    {
        get => _currentChat;
        set
        {
            this.RaiseAndSetIfChanged(ref _currentChat, value); 
            Console.WriteLine("CurrentChat changed to " + value);
        }
    }

    public MainWindowViewModel()
    {
        UsersField = new UsersFieldViewModel(this); 
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        await UsersField.LoadUsersAsync();

        // Initialize the chat to the first user in the list
        if (UsersField.Users.Count > 0)
        {
            UsersField.SelectedUser = UsersField.Users[0];
        }
    }
}