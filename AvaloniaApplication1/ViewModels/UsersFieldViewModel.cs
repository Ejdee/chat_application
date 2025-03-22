using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AvaloniaApplication1.Services;
using Firebase.Auth;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;

namespace AvaloniaApplication1.ViewModels;

public class UsersFieldViewModel : ViewModelBase
{
    public ObservableCollection<UserViewModel> Users { get; } = new ObservableCollection<UserViewModel>();
    private UserViewModel? _selectedUser;
    private readonly MainWindowViewModel _mainWindowViewModel;
    private readonly FirebaseService _firebaseService;
    private bool _isLoaded;

    public bool IsLoaded
    {
        get => _isLoaded;
        set => this.RaiseAndSetIfChanged(ref _isLoaded, value);
    }

    public UsersFieldViewModel(MainWindowViewModel mainWindowViewModel)
    {
        _firebaseService = App.ServiceProvider?.GetRequiredService<FirebaseService>() ?? new FirebaseService();
        _mainWindowViewModel = mainWindowViewModel;
    }

    public async Task LoadUsersAsync()
    {
        var users = await _firebaseService.GetUsersAsync();
        foreach (var user in users)
        {
            Users.Add(user);
        }

        SortUsersByActivity();
        
        _firebaseService.ListenForUserStatus(OnUserUpdated);
        await _firebaseService.ListenForChatUpdates(OnNewMessageIndicatorChanged);
        IsLoaded = true;
    }

    private void SortUsersByActivity()
    {
        var sortedUsers = Users.OrderByDescending(u => u.IsActive).ToList();
        
        Users.Clear();
        foreach (var user in sortedUsers)
        {
            Users.Add(user);
        }
    }

    private void OnUserUpdated(UserViewModel user, string action)
    {
        switch (action)
        {
            case "added":
                if (Users.All(u => u.Name != user.Name))
                {
                    if (user.IsActive)
                    {
                        // if active - add to the beginning of the list
                        Users.Insert(0, user);
                    }
                    else
                    {
                        // if offline - add to the end of the list
                        Users.Add(user);
                    }
                }

                break;
            case "modified":
                var modifiedUser = Users.FirstOrDefault(u => u.Name == user.Name);
                if (modifiedUser != null)
                {
                    Users.Remove(modifiedUser); 
                    
                    modifiedUser.Name = user.Name;
                    modifiedUser.IsActive = user.IsActive;
                    
                    // if active - add to the beginning of the list
                    // if offline - add to the end of the list
                    if (modifiedUser.IsActive) { Users.Insert(0, modifiedUser); }
                    else { Users.Add(modifiedUser); }
                }

                break;
            case "removed":
                var userToRemove = Users.FirstOrDefault(u => u.Name == user.Name);
                if (userToRemove != null)
                {
                    Users.Remove(userToRemove);
                }

                break;
        }
    }

    private void OnNewMessageIndicatorChanged(string username, bool displayIndicator)
    {
        var user = Users.FirstOrDefault(u => u.Name == username);
        if (user != null)
        {
            user.DisplayUnreadMessage = displayIndicator;
        }
    }

    public UserViewModel? SelectedUser
    {
        get => _selectedUser;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedUser, value);
            if (_selectedUser != null)
            {
                _ = OpenChatAsync(_selectedUser);
            }
        }
    }

    private async Task OpenChatAsync(UserViewModel user)
    {
        try
        {
            Console.WriteLine("Opening the chat with: " + user.Name);
            _mainWindowViewModel.CurrentChat = new ChatViewModel(user.Name);
            _mainWindowViewModel.CurrentChat.MessagesLoaded = false;
            
            List<MessageViewModel> messages = await _firebaseService.LoadChatAsync(user.Name);
            
            if (_mainWindowViewModel.CurrentChat.Messages != null)
            {
                // clear the old messages
                _mainWindowViewModel.CurrentChat.Messages.Clear();

                // add the new messages
                foreach (var message in messages)
                {
                    _mainWindowViewModel.CurrentChat.Messages.Add(message);
                }
                
                // indicate, that the messages are loaded
                _mainWindowViewModel.CurrentChat.MessagesLoaded = true;
                _firebaseService.ListenForMessageUpdates(_mainWindowViewModel.CurrentChat.OnMessageUpdated);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    public void OpenChatRandom()
    {
        var random = new Random();
        var index = random.Next(Users.Count);
        _ = OpenChatAsync(Users[index]);
    }
}

public class UserViewModel : ViewModelBase
{
    private string? _name;
    private bool _displayUnreadMessage;
    private Random _random = new();
    
    // List of colors to use for user avatars
    private static readonly List<string> Colors = new()
    {
        "#3498db", "#9b59b6", "#e74c3c", "#2ecc71", "#f1c40f",
        "#1abc9c", "#e67e22", "#34495e", "#95a5a6", "#d35400"
    };

    public bool DisplayUnreadMessage
    {
        get => _displayUnreadMessage;
        set => this.RaiseAndSetIfChanged(ref _displayUnreadMessage, value);
    }
    
    public string? Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    private bool _isActive;
    public bool IsActive
    {
        get => _isActive;
        set => this.RaiseAndSetIfChanged(ref _isActive, value);
    }

    public string Status => IsActive ? "Active" : "Offline";
    
    public string StatusColor => IsActive ? "#4CAF50" : "#9E9E9E";
    
    public string Initials 
    {
        get
        {
            if (string.IsNullOrEmpty(Name)) return "?";
            var parts = Name.Split(' ');
            string initials = "";
            foreach (var part in parts)
            {
                if (part.Length > 0)
                    initials += part[0];
            }
            return initials;
        }
    }
    
    // Get the random color from the list of colors
    public string AvatarColor => Colors[_random.Next(Colors.Count)];
}