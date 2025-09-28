using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ControlBeeWPF.ViewModels;
using Brushes = System.Windows.Media.Brushes;
using UserControl = System.Windows.Controls.UserControl;

namespace ControlBeeWPF.Views;

// ReSharper disable once InconsistentNaming
public partial class DigitalOutputStatusBarView : UserControl, IDisposable
{
    public static readonly DependencyProperty NameColumnWidthProperty =
        DependencyProperty.Register(
            nameof(NameColumnWidth),
            typeof(GridLength),
            typeof(DigitalOutputStatusBarView),
            new PropertyMetadata(new GridLength(1, GridUnitType.Auto)));

    private readonly DigitalOutputViewModel _viewModel;

    public DigitalOutputStatusBarView(DigitalOutputViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _viewModel.PropertyChanged += ViewModelOnPropertyChanged;
    }


    public GridLength NameColumnWidth
    {
        get => (GridLength)GetValue(NameColumnWidthProperty);
        set => SetValue(NameColumnWidthProperty, value);
    }

    public void Dispose()
    {
        _viewModel.Dispose();
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        NameLabel.Content = _viewModel.Name;
        ToolTip = _viewModel.ToolTip;
        ValueRect.Fill = _viewModel.Value is true ? Brushes.LawnGreen : Brushes.WhiteSmoke;
    }

    private void ValueLabel_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _viewModel.ToggleValue();
    }
}