using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using Avalonia.Controls;
using ChatApplication.Services;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;

namespace ChatApplication.ViewModels;

public class ChatViewModel : ViewModelBase
{ 
    private string? _username;
    private bool _messagesLoaded;
    private ObservableCollection<MessageViewModel>? _messages = new();
    private string? _newMessageText;
    private readonly FirebaseService _firebaseService;
    private ScrollViewer? _messageScrollViewer;

    public void SetScrollViewer(ScrollViewer scrollViewer)
    {
        _messageScrollViewer = scrollViewer;
    }
    
    public string? Username
    { 
        get => _username; 
        set => this.RaiseAndSetIfChanged(ref _username, value);
    }
    public bool MessagesLoaded
    {
        get => _messagesLoaded;
        set
        {
            this.RaiseAndSetIfChanged(ref _messagesLoaded, value);
            this.RaisePropertyChanged(nameof(ShowNoMessagesText));
        }
    }
    public ObservableCollection<MessageViewModel>? Messages
    {
        get => _messages;
        set
        {
            // unsubscribe from the old collection
            if (_messages != null) { _messages.CollectionChanged -= OnMessageChanged; }
            
            this.RaiseAndSetIfChanged(ref _messages, value);
            
            // subscribe to the new collection
            if (_messages != null)
            {
                _messages.CollectionChanged += (_, e) =>
                {
                    this.RaisePropertyChanged(nameof(ShowNoMessagesText));

                    if (e.Action == NotifyCollectionChangedAction.Add)
                    {
                        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                        {
                            _messageScrollViewer?.ScrollToEnd();
                        });
                    }
                };
            }
            
            this.RaisePropertyChanged(nameof(ShowNoMessagesText));
        } 
    }
    private void OnMessageChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // trigger the ShowNoMessagesText property to update
        this.RaisePropertyChanged(nameof(ShowNoMessagesText));
    }
    public bool ShowNoMessagesText => MessagesLoaded && (Messages == null || Messages.Count == 0);
    public string? NewMessageText
    {
        get => _newMessageText;
        set => this.RaiseAndSetIfChanged(ref _newMessageText, value);
    } 
    
    public ICommand SendCommand { get; }
    
    public ChatViewModel(string? username)
    {
        Username = username;
        
        _firebaseService = App.ServiceProvider?.GetRequiredService<FirebaseService>() ?? new FirebaseService();

        Messages = new ObservableCollection<MessageViewModel>();

        SendCommand = ReactiveCommand.Create(SendMessage);
    }

    public void OnMessageUpdated(MessageViewModel message, string change)
    {
        switch (change)
        {
            case "added":
                Messages?.Add(message);
                break;
            case "modified":
                var existingMessage = Messages?.FirstOrDefault(m => m.Timestamp == message.Timestamp);
                if (existingMessage != null) { existingMessage.Content = message.Content; }

                break;
            case "removed":
                var messageToRemove = Messages?.FirstOrDefault(m => m.Timestamp == message.Timestamp);
                if(messageToRemove != null) { Messages?.Remove(messageToRemove); }

                break;
        } 
    }
    
    public async void SendMessage()
    {
        try
        {
            // if the message is empty, don't send anything
            if (string.IsNullOrWhiteSpace(NewMessageText))
                return;

            var content = NewMessageText;
            // clear the field
            NewMessageText = "";

            try
            {
                await _firebaseService.SendMessageAsync(content);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error sending a message: {e}");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error sending a message: {e}");
        }
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
