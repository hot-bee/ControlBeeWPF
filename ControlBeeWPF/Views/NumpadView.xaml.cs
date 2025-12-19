using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using ControlBeeWPF.ViewModels;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace ControlBeeWPF.Views;

/// <summary>
///     Interaction logic for NumpadView.xaml
/// </summary>
public partial class NumpadView : Window
{
    private bool _closeByButton;
    private NumpadViewModel _viewModel;

    public NumpadView(NumpadViewModel viewModel)
    {
        _viewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();

        if (!_viewModel.HasValueLimit)
            ValueLimitGrid.Visibility = Visibility.Collapsed;
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
        {
            Enter();
        }
        else if (Equals(sender, CancelButton))
        {
            Cancel();
        }
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (DataContext is not NumpadViewModel vm)
            return;

        // Digits
        if (e.Key >= Key.D0 && e.Key <= Key.D9)
        {
            string digit = (e.Key - Key.D0).ToString();
            vm.InputDigitCommand.Execute(digit);
            e.Handled = true;
        }
        else if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
        {
            string digit = (e.Key - Key.NumPad0).ToString();
            vm.InputDigitCommand.Execute(digit);
            e.Handled = true;
        }
        // Dot
        else if (e.Key == Key.Decimal || e.Key == Key.OemPeriod)
        {
            if (vm.InputDotCommand.CanExecute(null))
                vm.InputDotCommand.Execute(null);
            e.Handled = true;
        }
        // Backspace
        else if (e.Key == Key.Back)
        {
            if (vm.BackspaceCommand.CanExecute(null))
                vm.BackspaceCommand.Execute(null);
            e.Handled = true;
        }
        // Clear: Escape or C
        else if (e.Key == Key.C || e.Key == Key.Escape)
        {
            vm.ClearCommand.Execute(null);
            e.Handled = true;
        }
        // Invert Sign: Plus or Minus
        else if (
            e.Key == Key.OemMinus
            || e.Key == Key.Subtract
            || e.Key == Key.OemPlus
            || e.Key == Key.Add
        )
        {
            if (vm.InvertSignCommand.CanExecute(null))
                vm.InvertSignCommand.Execute(null);
            e.Handled = true;
        }
        else if (e.Key == Key.Enter || e.Key == Key.Return)
        {
            Enter();
            e.Handled = true;
        }
    }
}
