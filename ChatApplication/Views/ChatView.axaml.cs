using Avalonia.Controls;
using ChatApplication.ViewModels;

namespace ChatApplication.Views;

public partial class ChatView : UserControl
{
    public ChatView()
    {
        InitializeComponent();
        this.AttachedToVisualTree += (_, _) => ConnectScrollViewer();
        this.DataContextChanged += (_, _) => ConnectScrollViewer();
    }

    private void ConnectScrollViewer()
    {
        if (DataContext is ChatViewModel viewModel)
        {
            viewModel.SetScrollViewer(MessageScrollViewer);
        }
    }
}