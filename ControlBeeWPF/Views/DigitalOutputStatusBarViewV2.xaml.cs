using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using ControlBeeWPF.ViewModels;
using Brushes = System.Windows.Media.Brushes;

namespace ControlBeeWPF.Views;

/// <summary>
///     Interaction logic for DigitalOutputStatusBarViewV2.xaml
/// </summary>
public partial class DigitalOutputStatusBarViewV2 : IDisposable
{
    public static readonly DependencyProperty NameColumnWidthProperty = DependencyProperty.Register(
        nameof(NameColumnWidth),
        typeof(GridLength),
        typeof(DigitalOutputStatusBarViewV2),
        new PropertyMetadata(new GridLength(90))
    );

    public static readonly DependencyProperty RowHeightProperty = DependencyProperty.Register(
        nameof(RowHeight),
        typeof(double),
        typeof(DigitalOutputStatusBarViewV2),
        new PropertyMetadata(30.0)
    );

    private readonly DigitalOutputViewModel _viewModel;

    public DigitalOutputStatusBarViewV2(DigitalOutputViewModel viewModel)
    {
        _viewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();
        viewModel.PropertyChanged += ViewModelOnPropertyChanged;
        UpdateView();
    }

    public GridLength NameColumnWidth
    {
        get => (GridLength)GetValue(NameColumnWidthProperty);
        set => SetValue(NameColumnWidthProperty, value);
    }

    public double RowHeight
    {
        get => (double)GetValue(RowHeightProperty);
        set => SetValue(RowHeightProperty, value);
    }

    public void Dispose()
    {
        _viewModel.Dispose();
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        UpdateView();
    }

    private void UpdateView()
    {
        NameText.Text = _viewModel.Name;
        ToolTip = _viewModel.ToolTip;
        var isOn = _viewModel.Value is true;
        ValueText.Text = isOn ? "On" : "Off";
        ValueBorder.Background = isOn ? Brushes.LawnGreen : Brushes.LightGray;
        ValueText.Foreground = isOn ? Brushes.Black : Brushes.DimGray;
    }

    private void ValueBorder_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _viewModel.ToggleValue();
    }
}
