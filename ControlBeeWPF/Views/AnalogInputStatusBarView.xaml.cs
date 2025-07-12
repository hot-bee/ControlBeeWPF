using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using ControlBeeWPF.ViewModels;

namespace ControlBeeWPF.Views;

public partial class AnalogInputStatusBarView : UserControl, IDisposable
{
    private readonly AnalogInputViewModel _viewModel;

    public AnalogInputStatusBarView(AnalogInputViewModel viewModel)
    {
        _viewModel = viewModel;
        InitializeComponent();
        viewModel.PropertyChanged += DigitalInputViewModelOnPropertyChanged;
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