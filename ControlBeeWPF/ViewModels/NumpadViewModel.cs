using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ControlBeeWPF.ViewModels;

// Written by GPT.
public partial class NumpadViewModel : ObservableObject
{
    [ObservableProperty]
    private string input;

    [ObservableProperty]
    private string? minValue;

    [ObservableProperty]
    private string? maxValue;

    private readonly bool allowDecimal;
    private readonly bool _isPasswordMode;
    private bool _hasUserInteracted = false;
    private string ClearValue => _isPasswordMode ? "" : "0";
    public bool HasValueLimit => MinValue != null || MaxValue != null;
    public string DisplayInput => _isPasswordMode ? new string('*', Input.Length) : Input;

    public NumpadViewModel(
        string initialValue = "0",
        bool allowDecimal = true,
        string? minValue = null,
        string? maxValue = null,
        bool isPasswordMode = false
    )
    {
        this.allowDecimal = allowDecimal;
        _isPasswordMode = isPasswordMode;
        Input = initialValue;
        MinValue = minValue;
        MaxValue = maxValue;
    }

    [RelayCommand]
    private void InputDigit(string digit)
    {
        if (!_hasUserInteracted)
        {
            Input = digit;
            _hasUserInteracted = true;
        }
        else if (Input == ClearValue)
        {
            Input = digit;
        }
        else
        {
            Input += digit;
        }
    }

    [RelayCommand(CanExecute = nameof(CanAddDot))]
    private void InputDot()
    {
        if (!_hasUserInteracted)
        {
            Input = "0.";
            _hasUserInteracted = true;
        }
        else
        {
            Input += ".";
        }
    }

    private bool CanAddDot() => !_isPasswordMode && allowDecimal && !Input.Contains(".");

    [RelayCommand(CanExecute = nameof(CanBackspace))]
    private void Backspace()
    {
        if (Input.Length > 1)
            Input = Input[..^1];
        else
            Input = ClearValue;

        _hasUserInteracted = true;
    }

    private bool CanBackspace() => !string.IsNullOrEmpty(Input) && Input != ClearValue;

    [RelayCommand]
    private void Clear()
    {
        Input = ClearValue;
        _hasUserInteracted = true;
    }

    [RelayCommand(CanExecute = nameof(CanInvertSign))]
    private void InvertSign()
    {
        if (double.TryParse(Input, out double value))
        {
            Input = (-value).ToString("G");
            _hasUserInteracted = true;
        }
    }

    private bool CanInvertSign() =>
        !_isPasswordMode && !string.IsNullOrWhiteSpace(Input) && Input != "0";

    partial void OnInputChanged(string value)
    {
        InputDotCommand.NotifyCanExecuteChanged();
        BackspaceCommand.NotifyCanExecuteChanged();
        InvertSignCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(DisplayInput));
    }
}
