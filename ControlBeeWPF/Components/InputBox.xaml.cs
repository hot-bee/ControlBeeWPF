using System.Windows;

namespace ControlBeeWPF.Components;

/// <summary>
///     Interaction logic for InputBox.xaml
/// </summary>
public partial class InputBox : Window
{
    // TODO: Remove this class after introducing Numpad
    public InputBox()
    {
        InitializeComponent();
    }

    public string ResponseText
    {
        get => ResponseTextBox.Text;
        set => ResponseTextBox.Text = value;
    }

    private void OKButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }
}
