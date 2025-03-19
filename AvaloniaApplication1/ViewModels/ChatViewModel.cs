using System;
using System.Linq;
using System.Net.Mime;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using AvaloniaApplication1.Views;
using ReactiveUI;

namespace AvaloniaApplication1.ViewModels;

public class ChatViewModel : ViewModelBase
{
     public ICommand BackToMainView { get;  }
     public string Username { get; }

     public ChatViewModel(string username)
     {
          Username = username;
          BackToMainView = ReactiveCommand.Create(BackToMain);
     }

     private void BackToMain()
     {
          throw new NotImplementedException();
     }
}