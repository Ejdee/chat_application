using System;
using AvaloniaApplication1.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using ReactiveUI;

namespace AvaloniaApplication1.ViewModels;

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
        CurrentChat = new ChatViewModel("Adam");
    }
}