using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace ControlBeeWPF.ViewModels;

public partial class KeyboardViewModel : ObservableObject
{
    [ObservableProperty]
    private string input = "";

    private bool _hasUserInteracted;
    
    public ObservableCollection<string> Keys { get; } =
        new(Enumerable.Range('A', 26).Select(c => ((char)c).ToString()));

    public KeyboardViewModel(string initialValue = "", bool startUpper = true)
    {
        Input = initialValue ?? "";
        _hasUserInteracted = !string.IsNullOrEmpty(Input);
    }

    [RelayCommand]
    private void InputLetter(string letter)
    {
        if (string.IsNullOrEmpty(letter))
            return;

        char ch = letter[0];
        if (!char.IsLetter(ch))
            return;

        if (!_hasUserInteracted)
        {
            Input = "";
            _hasUserInteracted = true;
        }

        Input += ch;
    }

    [RelayCommand(CanExecute = nameof(CanBackspace))]
    private void Backspace()
    {
        if (string.IsNullOrEmpty(Input))
            return;

        Input = Input.Length > 1 ? Input[..^1] : "";
        _hasUserInteracted = true;
    }

    private bool CanBackspace() => !string.IsNullOrEmpty(Input);

    [RelayCommand]
    private void Clear()
    {
        Input = "";
        _hasUserInteracted = true;
    }

    partial void OnInputChanged(string value)
    {
        BackspaceCommand.NotifyCanExecuteChanged();
    }
}