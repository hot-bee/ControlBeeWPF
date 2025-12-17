using ControlBeeWPF.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace ControlBeeWPF.Views;

/// <summary>
///     Interaction logic for KeyboardView.xaml
/// </summary>
public partial class KeyboardView
{
    private bool _closeByButton;
    private readonly KeyboardViewModel _viewModel;

    public KeyboardView(KeyboardViewModel viewModel)
    {
        _viewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();
    }

    public string Value => _viewModel.Input;

    protected override void OnClosing(CancelEventArgs e)
    {
        if (!_closeByButton)
            DialogResult = false;

        base.OnClosing(e);
    }

    private void Enter()
    {
        _closeByButton = true;
        DialogResult = true;
        Close();
    }

    private void Cancel()
    {
        _closeByButton = true;
        DialogResult = false;
        Close();
    }

    private void Button_OnClick(object sender, RoutedEventArgs e)
    {
        if (Equals(sender, EnterButton))
            Enter();
        else if (Equals(sender, CancelButton))
            Cancel();
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (DataContext is not KeyboardViewModel viewModel)
            return;

        if (e.Key is Key.Enter or Key.Return)
        {
            Enter();
            e.Handled = true;
            return;
        }

        if (e.Key is Key.Escape)
        {
            Cancel();
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Back)
        {
            if (viewModel.BackspaceCommand.CanExecute(null))
                viewModel.BackspaceCommand.Execute(null);

            e.Handled = true;
            return;
        }

        if (e.Key == Key.Delete || (e.Key == Key.Back && (Keyboard.Modifiers & ModifierKeys.Control) != 0))
        {
            viewModel.ClearCommand.Execute(null);
            e.Handled = true;
            return;
        }

        if (e.Key is >= Key.A and <= Key.Z)
        {
            char key = (char)('A' + (e.Key - Key.A));
            bool shift = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;
            string letter = shift ? key.ToString() : char.ToLowerInvariant(key).ToString();

            viewModel.InputLetterCommand.Execute(letter);
            e.Handled = true;
            return;
        }
    }
}