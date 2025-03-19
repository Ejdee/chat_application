using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ReactiveUI;

namespace AvaloniaApplication1.ViewModels;

public class UsersFieldViewModel : ViewModelBase
{
    public ObservableCollection<UserViewModel> Users { get; }
    private UserViewModel? _selectedUser;

    public UsersFieldViewModel()
    {
        Users = new ObservableCollection<UserViewModel>
        {
            new UserViewModel { Name = "John Doe", IsActive = true },
            new UserViewModel { Name = "Jane Smith", IsActive = true },
            new UserViewModel { Name = "Bob Johnson", IsActive = false },
            new UserViewModel { Name = "Alice Brown", IsActive = true },
            new UserViewModel { Name = "Mike Wilson", IsActive = false },
            new UserViewModel { Name = "Adam Baker", IsActive = true }
        };
    }

    public UserViewModel? SelectedUser
    {
        get => _selectedUser;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedUser, value);
            if (_selectedUser != null)
            {
                OpenChat(_selectedUser);
            }
        }
    }

    private void OpenChat(UserViewModel user)
    {
        Console.WriteLine("Opening the chat with: " + user.Name);        
    }
    
    public void OpenChatRandom()
    {
        var random = new Random();
        var index = random.Next(Users.Count);
        OpenChat(Users[index]);
    }
}

public class UserViewModel : ViewModelBase
{
    private string _name;
    
    // List of colors to use for user avatars
    private static readonly List<string> Colors = new()
    {
        "#3498db", "#9b59b6", "#e74c3c", "#2ecc71", "#f1c40f",
        "#1abc9c", "#e67e22", "#34495e", "#95a5a6", "#d35400"
    };

    private Random _random = new();
    
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    private bool _isActive;
    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
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