using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using ControlBeeWPF.ViewModels;
using UserControl = System.Windows.Controls.UserControl;

namespace ControlBeeWPF.Views;

public partial class AnalogInputStatusBarView : UserControl, IDisposable
{
    public static readonly DependencyProperty NameColumnWidthProperty =
        DependencyProperty.Register(
            nameof(NameColumnWidth),
            typeof(GridLength),
            typeof(AnalogInputStatusBarView),
            new PropertyMetadata(new GridLength(1, GridUnitType.Auto)));

    private readonly AnalogInputViewModel _viewModel;

    public AnalogInputStatusBarView(AnalogInputViewModel viewModel)
    {
        _viewModel = viewModel;
        InitializeComponent();
        viewModel.PropertyChanged += DigitalInputViewModelOnPropertyChanged;
    }

    public GridLength NameColumnWidth
    {
        get => (GridLength)GetValue(NameColumnWidthProperty);
        set => SetValue(NameColumnWidthProperty, value);
    }

    public bool ShowName
    {
        get => NameLabel.Visibility == Visibility.Visible;
        set => NameLabel.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
    }

    public void Dispose()
    {
        _viewModel.Dispose();
    }

    private void DigitalInputViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        NameLabel.Content = _viewModel.Name;
        ValueLabel.Content = _viewModel.Value;
    }
}