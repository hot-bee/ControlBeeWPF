using System.ComponentModel;
using System.Windows;

namespace ControlBeeWPF.Views;

/// <summary>
///     Interaction logic for NumpadView.xaml
/// </summary>
public partial class NumpadView : Window
{
    private readonly string _initialValue;
    private readonly long?[] _valueParts = new long?[2];
    private bool _closeByButton;
    private bool _edited;
    private long _focusOnParts;
    private readonly bool _useFractional;

    public NumpadView(object initialValue)
    {
        InitializeComponent();
        _initialValue = initialValue.ToString() ?? "0";
        if (initialValue is double)
            _useFractional = true;
        ValueText.Text = Value;
        PointButton.IsEnabled = _useFractional;
    }

    private string AssembledValue =>
        _useFractional
            ? $"{_valueParts[0] ?? 0:N0}.{_valueParts[1] ?? 0}"
            : $"{_valueParts[0] ?? 0:N0}";

    public string Value => _edited ? AssembledValue : _initialValue;

    protected override void OnClosing(CancelEventArgs e)
    {
        if (!_closeByButton)
            DialogResult = false;
        base.OnClosing(e);
    }

    private void UpdateValue()
    {
        _edited = true;
        ValueText.Text = Value;
    }

    private void ClickNumber(int digit)
    {
        if ((_valueParts[_focusOnParts] ?? 0) > long.MaxValue / 20)
            return;
        _valueParts[_focusOnParts] = (_valueParts[_focusOnParts] ?? 0) * 10;
        _valueParts[_focusOnParts] += digit;
        UpdateValue();
    }

    private void Button_OnClick(object sender, RoutedEventArgs e)
    {
        if (Equals(sender, EnterButton))
        {
            _closeByButton = true;
            DialogResult = true;
            Close();
        }
        else if (Equals(sender, CancelButton))
        {
            _closeByButton = true;
            DialogResult = false;
            Close();
        }
        else if (Equals(sender, MinusButton))
        {
            _valueParts[0] *= -1;
            UpdateValue();
        }
        else if (Equals(sender, ClearButton))
        {
            _valueParts[0] = null;
            _valueParts[1] = null;
            _focusOnParts = 0;
            UpdateValue();
        }
        else if (Equals(sender, BackButton))
        {
            if (_focusOnParts == 1 && _valueParts[_focusOnParts] == 0)
                _focusOnParts = 0;
            _valueParts[_focusOnParts] = (_valueParts[_focusOnParts] ?? 0) / 10;
            UpdateValue();
        }
        else if (Equals(sender, PointButton))
        {
            if (_focusOnParts == 0)
                _focusOnParts = 1;
        }
        else if (Equals(sender, ThousandButton))
        {
            if ((_valueParts[_focusOnParts] ?? 0) > long.MaxValue / 1000)
                return;
            _valueParts[_focusOnParts] = (_valueParts[_focusOnParts] ?? 0) * 1000;
            UpdateValue();
        }
        else if (Equals(sender, ZeroButton))
        {
            ClickNumber(0);
        }
        else if (Equals(sender, OneButton))
        {
            ClickNumber(1);
        }
        else if (Equals(sender, TwoButton))
        {
            ClickNumber(2);
        }
        else if (Equals(sender, ThreeButton))
        {
            ClickNumber(3);
        }
        else if (Equals(sender, FourButton))
        {
            ClickNumber(4);
        }
        else if (Equals(sender, FiveButton))
        {
            ClickNumber(5);
        }
        else if (Equals(sender, SixButton))
        {
            ClickNumber(6);
        }
        else if (Equals(sender, SevenButton))
        {
            ClickNumber(7);
        }
        else if (Equals(sender, EightButton))
        {
            ClickNumber(8);
        }
        else if (Equals(sender, NineButton))
        {
            ClickNumber(9);
        }
    }
}
