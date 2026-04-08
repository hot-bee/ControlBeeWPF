using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using ControlBeeWPF.Interfaces;
using ControlBeeWPF.ViewModels;
using UserControl = System.Windows.Controls.UserControl;

namespace ControlBeeWPF.Views;

public partial class AnalogOutputStatusBarView : UserControl, IDisposable
{
    public static readonly DependencyProperty NameColumnWidthProperty = DependencyProperty.Register(
        nameof(NameColumnWidth),
        typeof(GridLength),
        typeof(AnalogOutputStatusBarView),
        new PropertyMetadata(new GridLength(1, GridUnitType.Auto))
    );

    private readonly AnalogOutputViewModel _viewModel;
    private readonly IViewFactory _viewFactory;

    public AnalogOutputStatusBarView(AnalogOutputViewModel viewModel, IViewFactory viewFactory)
    {
        _viewModel = viewModel;
        _viewFactory = viewFactory;
        InitializeComponent();
        viewModel.PropertyChanged += ViewModelOnPropertyChanged;
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

    public Func<object?, object?>? ValueConverter { get; set; }

    public void Dispose()
    {
        _viewModel.Dispose();
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        NameLabel.Content = _viewModel.Name;
        var value = _viewModel.Value;
        ValueLabel.Content = ValueConverter != null ? ValueConverter(value) : value;
    }

    private void ValueLabel_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var initialValue = _viewModel.Value?.ToString() ?? "0";
        var numpad = _viewFactory.Create<NumpadView>(initialValue, true);
        if (numpad!.ShowDialog() is not true)
            return;
        if (!double.TryParse(numpad.Value.Replace(",", ""), out var newValue))
            return;
        _viewModel.SetValue(newValue);
    }
}
