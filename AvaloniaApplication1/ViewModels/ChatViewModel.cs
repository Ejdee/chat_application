using System;
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

     public ChatViewModel(string? username)
     {
          Console.WriteLine("Setting the name to the : " + username);
          Username = username;
     }
}