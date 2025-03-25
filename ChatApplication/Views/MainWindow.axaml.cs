using System;
using Avalonia.Controls;
using ChatApplication.Services;
using ChatApplication.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ChatApplication.Views;

public partial class MainWindow : Window
{
    private bool _isClosing = false; 
    
    public MainWindow()
    {
        InitializeComponent();
        DataContext=  new MainWindowViewModel();
        this.Closing += MainWindowClosing;
    }

    private async void MainWindowClosing(object? sender, WindowClosingEventArgs e)
    {
        if (_isClosing)
        {
            return;
        } 
        
        _isClosing = true;
        
        // cancel the closing
        e.Cancel = true; 
        
        var firebaseService = App.ServiceProvider?.GetService<FirebaseService>();
        if (firebaseService != null)
        {
            try
            {
                Console.WriteLine("EXITING MAIN WINDOW PROCESS"); 
                
                Console.WriteLine("Clearing the currently reading");
                await firebaseService.ClearTheCurrentlyReadingAsync();
                Console.WriteLine("Cleared the currently reading");
                
                Console.WriteLine("Logging out user");
                await firebaseService.LogOutUserAsync();
                Console.WriteLine("Logged out");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error on exiting the program: " + ex);
            }
            finally
            {
                Close();
            }
        }
        else
        {
            Close();
        }
    }
}