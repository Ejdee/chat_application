using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ChatApplication.Views;

public partial class UsersField : UserControl
{
    public UsersField()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}