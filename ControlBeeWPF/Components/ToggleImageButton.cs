using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Button = System.Windows.Controls.Button;
using Image = System.Windows.Controls.Image;
using Orientation = System.Windows.Controls.Orientation;

namespace ControlBeeWPF.Components;

public class ToggleImageButton : Button, INotifyPropertyChanged
{
    private readonly StackPanel _offContent;
    private readonly StackPanel _onContent;
    private bool _isChecked;

    public ToggleImageButton(BitmapImage offImage, BitmapImage onImage, string text)
    {
        _offContent = new StackPanel { Orientation = Orientation.Horizontal };
        _offContent.Children.Add(
            new Image
            {
                Source = offImage,
                Width = 16,
                Height = 16,
            }
        );
        _offContent.Children.Add(
            new TextBlock
            {
                Text = text,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(5, 0, 0, 0),
            }
        );

        _onContent = new StackPanel { Orientation = Orientation.Horizontal };
        _onContent.Children.Add(
            new Image
            {
                Source = onImage,
                Width = 16,
                Height = 16,
            }
        );
        _onContent.Children.Add(
            new TextBlock
            {
                Text = text,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(5, 0, 0, 0),
            }
        );

        Content = _offContent;
    }

    public bool IsChecked
    {
        get => _isChecked;
        set
        {
            if (SetField(ref _isChecked, value))
                Content = _isChecked ? _onContent : _offContent;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected override void OnClick()
    {
        base.OnClick();
        IsChecked = !IsChecked;
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
