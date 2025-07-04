using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ControlBee.Interfaces;
using ControlBeeWPF.ViewModels;

namespace ControlBeeWPF.Views;

// ReSharper disable once InconsistentNaming
public partial class DigitalOutputStatusBarView : UserControl, IDisposable
{
    private readonly DigitalOutputViewModel _viewModel;

    public DigitalOutputStatusBarView(IActorRegistry actorRegistry, string actorName, string itemPath)
    {
        InitializeComponent();
        _viewModel = new DigitalOutputViewModel(actorRegistry, actorName, itemPath);
        _viewModel.PropertyChanged += ViewModelOnPropertyChanged;
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

    private void ValueRect_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _viewModel.ToggleValue();
    }
}