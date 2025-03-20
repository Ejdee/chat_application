using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ReactiveUI;

namespace AvaloniaApplication1.ViewModels;

public class ChatViewModel : ViewModelBase
{ 
    private string? _username;
    public string? Username
    { 
        get => _username; 
        set => this.RaiseAndSetIfChanged(ref _username, value);
    }

    private ObservableCollection<MessageViewModel>? _messages = new();
    public ObservableCollection<MessageViewModel>? Messages
    {
        get => _messages;
        set => this.RaiseAndSetIfChanged(ref _messages, value);
    }

    private string? _newMessageText;
    public string? NewMessageText
    {
        get => _newMessageText;
        set => this.RaiseAndSetIfChanged(ref _newMessageText, value);
    } 
    
    public ICommand SendCommand { get; }
    
    public ChatViewModel(string? username)
    { 
        Username = username;
        
        Messages = new ObservableCollection<MessageViewModel>
        {
            new MessageViewModel { Content = "Hello!", Timestamp = "12:00", IsOwnMessage = false }, 
            new MessageViewModel { Content = "Hi!", Timestamp = "12:01", IsOwnMessage = true }, 
            new MessageViewModel
             { Content = "How are you doing, my friendly friend?", Timestamp = "12:02", IsOwnMessage = false }, 
            new MessageViewModel 
            { 
                Content = "I am doing really good actually, today I was hungry so i cooked some shit!", 
                Timestamp = "12:03", IsOwnMessage = true 
            }, 
        };

        SendCommand = ReactiveCommand.Create(SendMessage);
    }
    
    public void SendMessage()
    {
        // if the message is empty, don't send anything
        if (string.IsNullOrWhiteSpace(NewMessageText))
            return;

        string messageText = NewMessageText;
        
        // add the message to the list of messages
        Messages?.Add(new MessageViewModel
        {
            Content = messageText,
            Timestamp = DateTime.Now.ToString("HH:mm"),
            IsOwnMessage = true
        });
        NewMessageText = "";
    }    
}

public class MessageViewModel : ViewModelBase
{
    private string _content;
    private string _timestamp;
    private bool _isOwnMessage;
    
    public string Content
    {
        get => _content;
        set => this.RaiseAndSetIfChanged(ref _content, value);
    }

    public string Timestamp
    {
        get => _timestamp;
        set => this.RaiseAndSetIfChanged(ref _timestamp, value);
    }
    
    public bool IsOwnMessage
    {
        get => _isOwnMessage;
        set => this.RaiseAndSetIfChanged(ref _isOwnMessage, value);
    }

}
