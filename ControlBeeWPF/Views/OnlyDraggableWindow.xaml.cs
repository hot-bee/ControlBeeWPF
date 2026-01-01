using System.Windows;
using System.Windows.Input;

namespace ControlBeeWPF.Views;

/// <summary>
///     Interaction logic for OnlyDraggableWindow.xaml
/// </summary>
public partial class OnlyDraggableWindow : Window
{
    public OnlyDraggableWindow()
    {
        InitializeComponent();
    }

    public new object? Content
    {
        get => MyContent.Content;
        set => MyContent.Content = value;
    }

    public string TitleBarText
    {
        get => TitleBarTextBlock.Text;
        set => TitleBarTextBlock.Text = value;
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
            DragMove(); // Allows the window to move
    }
}
