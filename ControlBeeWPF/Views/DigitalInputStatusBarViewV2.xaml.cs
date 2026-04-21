using System.ComponentModel;
using System.Windows;
using ControlBeeWPF.ViewModels;
using Brushes = System.Windows.Media.Brushes;

namespace ControlBeeWPF.Views;

/// <summary>
///     Interaction logic for DigitalInputStatusBarViewV2.xaml
/// </summary>
public partial class DigitalInputStatusBarViewV2 : IDisposable
{
    public static readonly DependencyProperty NameColumnWidthProperty = DependencyProperty.Register(
        nameof(NameColumnWidth),
        typeof(GridLength),
        typeof(DigitalInputStatusBarViewV2),
        new PropertyMetadata(new GridLength(7, GridUnitType.Star))
    );

    public static readonly DependencyProperty RowHeightProperty = DependencyProperty.Register(
        nameof(RowHeight),
        typeof(double),
        typeof(DigitalInputStatusBarViewV2),
        new PropertyMetadata(30.0)
    );

    private readonly DigitalInputViewModel _viewModel;

    public DigitalInputStatusBarViewV2(DigitalInputViewModel viewModel)
    {
        _viewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();
        viewModel.PropertyChanged += DigitalInputViewModelOnPropertyChanged;
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

    private void DigitalInputViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        UpdateView();
    }

    private void UpdateView()
    {
        NameText.Text = _viewModel.Name;
        var isOn = _viewModel.Value is true;
        ValueText.Text = isOn ? "On" : "Off";
        ValueBorder.Background = isOn ? Brushes.Red : Brushes.LightGray;
        ValueText.Foreground = isOn ? Brushes.White : Brushes.DimGray;
    }
}
