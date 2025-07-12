using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;
using ControlBeeWPF.ViewModels;

namespace ControlBeeWPF.Views;

/// <summary>
///     Interaction logic for IOStatusBar.xaml
/// </summary>
// ReSharper disable once InconsistentNaming
public partial class DigitalInputStatusBarView : UserControl, IDisposable
{
    private readonly DigitalInputViewModel _viewModel;

    public DigitalInputStatusBarView(DigitalInputViewModel viewModel)
    {
        _viewModel = viewModel;
        InitializeComponent();
        viewModel.PropertyChanged += DigitalInputViewModelOnPropertyChanged;
    }

    public void Dispose()
    {
        _viewModel.Dispose();
    }

    private void DigitalInputViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        NameLabel.Content = _viewModel.Name;
        ValueRect.Fill = _viewModel.Value is true ? Brushes.OrangeRed : Brushes.WhiteSmoke;
    }
}