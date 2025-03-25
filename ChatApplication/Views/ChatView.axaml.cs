using Avalonia.Controls;
using ChatApplication.ViewModels;

namespace ChatApplication.Views;

public partial class ChatView : UserControl
{
    public ChatView()
    {
        InitializeComponent();
        this.AttachedToVisualTree += (s, e) => ConnectScrollViewer();
        this.DataContextChanged += (s, e) => ConnectScrollViewer();
    }

    private void ConnectScrollViewer()
    {
        if (DataContext is ChatViewModel viewModel)
        {
            viewModel.SetScrollViewer(MessageScrollViewer);
        }
    }
}