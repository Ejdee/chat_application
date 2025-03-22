using Avalonia.Controls;
using AvaloniaApplication1.ViewModels;

namespace AvaloniaApplication1.Views;

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