using System.Windows;
using System.Windows.Input;
using ControlBeeWPF.Interfaces;
using MessageBox = System.Windows.MessageBox;

namespace ControlBeeWPF.Views;

/// <summary>
///     Interaction logic for LotoPasswordInputView.xaml
/// </summary>
public partial class LotoPasswordInputView : Window
{
    private readonly IViewFactory _viewFactory;

    public string Password = string.Empty;

    public LotoPasswordInputView(IViewFactory viewFactory)
    {
        _viewFactory = viewFactory;
        InitializeComponent();
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        if (Equals(sender, OkButton))
        {
            if (Math.Min(PasswordBox.Password.Length, ConfirmPasswordBox.Password.Length) < 4)
            {
                MessageBox.Show(
                    "Password must be at least 4 characters.",
                    "Password Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            if (
                PasswordBox.Password != string.Empty
                && PasswordBox.Password == ConfirmPasswordBox.Password
            )
            {
                Password = PasswordBox.Password;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show(
                    "Passwords do not match. Please try again.",
                    "Password Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
        }
        else if (Equals(sender, CancelButton))
        {
            DialogResult = false;
            Close();
        }
    }

    private void PasswordBox_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var keyboardView = _viewFactory.Create<KeyboardView>()!;
        if (keyboardView.ShowDialog() is not true)
            return;

        if (Equals(sender, PasswordBox))
            PasswordBox.Password = keyboardView.Value;
        else if (Equals(sender, ConfirmPasswordBox))
            ConfirmPasswordBox.Password = keyboardView.Value;
    }
}
