using System.ComponentModel;
using System.Windows;
using ControlBeeWPF.ViewModels;
using Brushes = System.Windows.Media.Brushes;
using UserControl = System.Windows.Controls.UserControl;

namespace ControlBeeWPF.Views;

/// <summary>
///     Interaction logic for IOStatusBar.xaml
/// </summary>
// ReSharper disable once InconsistentNaming
public partial class DigitalInputStatusBarView : UserControl, IDisposable
{
    public static readonly DependencyProperty NameColumnWidthProperty = DependencyProperty.Register(
        nameof(NameColumnWidth),
        typeof(GridLength),
        typeof(DigitalInputStatusBarView),
        new PropertyMetadata(new GridLength(1, GridUnitType.Auto))
    );

    private readonly DigitalInputViewModel _viewModel;

    public DigitalInputStatusBarView(DigitalInputViewModel viewModel)
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
        NameLabel.Content = _viewModel.Name;
        ValueRect.Fill = _viewModel.Value is true ? Brushes.OrangeRed : Brushes.WhiteSmoke;
    }
}
