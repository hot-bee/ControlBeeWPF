using System.Windows;

namespace ControlBeeWPF.Views;

/// <summary>
///     Interaction logic for LoadingMainWindowView.xaml
/// </summary>
public partial class LoadingMainWindowView : Window
{
    public LoadingMainWindowView(string? message = null)
    {
        InitializeComponent();
        if (message != null)
            LoadingMessageText.Text = message;
    }
}
