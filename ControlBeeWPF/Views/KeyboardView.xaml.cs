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
            if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                viewModel.ClearCommand.Execute(null);
            }
            else
            {
                if (viewModel.BackspaceCommand.CanExecute(null))
                    viewModel.BackspaceCommand.Execute(null);
            }

            e.Handled = true;
            return;
        }

        if (e.Key == Key.Delete)
        {
            viewModel.ClearCommand.Execute(null);
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Space)
        {
            viewModel.InputTextCommand.Execute(" ");
            e.Handled = true;
            return;
        }

        if (e.Key == Key.CapsLock)
        {
            viewModel.ToggleCapsCommand.Execute(null);
            e.Handled = true;
            return;
        }

        if (e.Key is >= Key.D0 and <= Key.D9)
        {
            char ch = (char)('0' + (e.Key - Key.D0));
            viewModel.InputTextCommand.Execute(ch.ToString());
            e.Handled = true;
            return;
        }

        if (e.Key is >= Key.A and <= Key.Z)
        {
            char baseCh = (char)('a' + (e.Key - Key.A));
            bool shiftDown = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;

            bool upper = viewModel.IsCaps ^ shiftDown;
            char outCh = upper ? char.ToUpperInvariant(baseCh) : baseCh;

            viewModel.InputTextCommand.Execute(outCh.ToString());
            e.Handled = true;
            return;
        }

        string? oem = e.Key switch
        {
            Key.OemMinus => "-",
            Key.OemPlus => "=",
            Key.OemComma => ",",
            Key.OemPeriod => ".",
            Key.OemQuestion => "?",
            Key.Oem1 => ";",
            Key.OemQuotes => "\"",
            Key.OemOpenBrackets => "[",
            Key.Oem6 => "]",
            Key.Oem5 => "\\",
            _ => null
        };

        if (oem is not null)
        {
            viewModel.InputTextCommand.Execute(oem);
            e.Handled = true;
        }
    }
}