using AvaloniaApplication1.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using ReactiveUI;

namespace AvaloniaApplication1.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private ViewModelBase _currentChat;
    private UserViewModel? _selectedUser;

    public UserViewModel? SelectedUser
    {
        get => _selectedUser;
        set => this.RaiseAndSetIfChanged(ref _selectedUser, value);
    }
    
    public ViewModelBase CurrentChat
    {
        get => _currentChat;
        set => this.RaiseAndSetIfChanged(ref _currentChat, value);
    }
    
    private readonly ChatViewModel _chatPage = new ChatViewModel("Adam");

    public MainWindowViewModel()
    {
        CurrentChat = _chatPage;
    }
    
    
}