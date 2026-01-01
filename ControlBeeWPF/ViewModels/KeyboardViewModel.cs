using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ControlBeeWPF.ViewModels;

public partial class KeyboardViewModel : ObservableObject
{
    public sealed class KeyItem
    {
        public string Kind { get; init; } = "Char";
        public string Label { get; init; } = "";
        public string Value { get; init; } = "";
        public bool IsWide { get; init; }
    }

    public enum LayoutMode
    {
        Abc,
        Num,
        Sym,
    }

    [ObservableProperty]
    private string _input = "";

    [ObservableProperty]
    private bool _isShift;

    [ObservableProperty]
    private bool _isCaps;

    [ObservableProperty]
    private LayoutMode _mode = LayoutMode.Abc;

    private bool _hasUserInteracted;

    public ObservableCollection<ObservableCollection<KeyItem>> Rows { get; } = new();

    public KeyboardViewModel(string initialValue = "")
    {
        Input = initialValue ?? "";
        _hasUserInteracted = !string.IsNullOrEmpty(Input);

        IsCaps = false;
        IsShift = false;
        Mode = LayoutMode.Abc;

        Rebuild();
    }

    partial void OnIsShiftChanged(bool value) => Rebuild();

    partial void OnIsCapsChanged(bool value) => Rebuild();

    partial void OnModeChanged(LayoutMode value) => Rebuild();

    [RelayCommand]
    private void PressKey(KeyItem key)
    {
        switch (key.Kind)
        {
            case "Char":
                InputText(key.Value);
                IsShift = false;
                break;

            case "Space":
                InputText(" ");
                IsShift = false;
                break;

            case "Back":
                Backspace();
                break;

            case "Shift":
                IsShift = !IsShift;
                break;

            case "Caps":
                IsCaps = !IsCaps;
                break;

            case "ABC":
                Mode = LayoutMode.Abc;
                break;

            case "NUM":
                Mode = LayoutMode.Num;
                break;

            case "SYM":
                Mode = LayoutMode.Sym;
                break;
        }
    }

    [RelayCommand]
    public void InputText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        if (!_hasUserInteracted)
        {
            Input = "";
            _hasUserInteracted = true;
        }

        Input += text;
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

    [RelayCommand]
    public void ToggleCaps() => IsCaps = !IsCaps;

    partial void OnInputChanged(string value)
    {
        BackspaceCommand.NotifyCanExecuteChanged();
    }

    private void Rebuild()
    {
        Rows.Clear();

        if (Mode == LayoutMode.Abc)
            BuildAbc();
        else if (Mode == LayoutMode.Num)
            BuildNum();
        else
            BuildSym();
    }

    private void BuildAbc()
    {
        Rows.Add(Row(Chars("qwertyuiop")));
        Rows.Add(Row(Chars("asdfghjkl")));

        var row = new List<KeyItem>
        {
            new() { Kind = "Shift", Label = "Shift" },
        };
        row.AddRange(Chars("zxcvbnm"));
        row.Add(new KeyItem { Kind = "Back", Label = "⌫" });
        Rows.Add(Row(row.ToArray()));

        Rows.Add(
            Row(
                new KeyItem { Kind = "Caps", Label = "Caps" },
                new KeyItem { Kind = "NUM", Label = "123" },
                new KeyItem { Kind = "SYM", Label = "#+=" },
                new KeyItem
                {
                    Kind = "Space",
                    Label = "Space",
                    IsWide = true,
                }
            )
        );
    }

    private void BuildNum()
    {
        Rows.Add(Row(Chars("1234567890", applyCase: false)));

        Rows.Add(Row(Chars(@"-/:;()₩&@""", applyCase: false)));

        var row = new List<KeyItem>
        {
            new() { Kind = "SYM", Label = "#+=" },
        };
        row.AddRange(Chars(".,?!'", applyCase: false));
        row.Add(new KeyItem { Kind = "Back", Label = "⌫" });
        Rows.Add(Row(row.ToArray()));

        Rows.Add(
            Row(
                new KeyItem { Kind = "ABC", Label = "ABC" },
                new KeyItem
                {
                    Kind = "Space",
                    Label = "Space",
                    IsWide = true,
                }
            )
        );
    }

    private void BuildSym()
    {
        Rows.Add(Row(Chars("[]{}#%^*+=", applyCase: false)));
        Rows.Add(Row(Chars(@"_\|~<>€£¥•", applyCase: false)));

        var row = new List<KeyItem>
        {
            new() { Kind = "NUM", Label = "123" },
        };
        row.AddRange(Chars(".,?!'", applyCase: false));
        row.Add(new KeyItem { Kind = "Back", Label = "⌫" });
        Rows.Add(Row(row.ToArray()));

        Rows.Add(
            Row(
                new KeyItem { Kind = "ABC", Label = "ABC" },
                new KeyItem
                {
                    Kind = "Space",
                    Label = "Space",
                    IsWide = true,
                }
            )
        );
    }

    private static ObservableCollection<KeyItem> Row(params KeyItem[] keys) => new(keys);

    private KeyItem[] Chars(string s, bool applyCase = true)
    {
        var list = new List<KeyItem>(s.Length);
        foreach (var ch in s)
        {
            string outCh = applyCase ? ApplyCase(ch) : ch.ToString();
            list.Add(
                new KeyItem
                {
                    Kind = "Char",
                    Label = outCh,
                    Value = outCh,
                }
            );
        }
        return list.ToArray();
    }

    private string ApplyCase(char ch)
    {
        if (Mode != LayoutMode.Abc)
            return ch.ToString();

        bool upper = IsCaps ^ IsShift;
        char outCh = upper ? char.ToUpperInvariant(ch) : char.ToLowerInvariant(ch);
        return outCh.ToString();
    }
}
