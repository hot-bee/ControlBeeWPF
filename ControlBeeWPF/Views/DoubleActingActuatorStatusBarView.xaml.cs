using System.ComponentModel;
using System.Windows.Input;
using ControlBee.Interfaces;
using ControlBeeWPF.ViewModels;
using Brushes = System.Windows.Media.Brushes;

namespace ControlBeeWPF.Views;

// ReSharper disable once InconsistentNaming
public partial class DoubleActingActuatorStatusBarView : IDisposable
{
    private readonly DoubleActingActuatorViewModel _viewModel;

    public DoubleActingActuatorStatusBarView(
        IActorRegistry actorRegistry,
        string actorName,
        string itemPath
    )
        : this(new DoubleActingActuatorViewModel(actorRegistry, actorName, itemPath)) { }

    public DoubleActingActuatorStatusBarView(DoubleActingActuatorViewModel viewModel)
    {
        _viewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();
        viewModel.PropertyChanged += ViewModelOnPropertyChanged;
        UpdateView();
    }

    public void Dispose()
    {
        _viewModel.PropertyChanged -= ViewModelOnPropertyChanged;
        _viewModel.Dispose();
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        UpdateView();
    }

    private void UpdateView()
    {
        NameLabel.Content = _viewModel.Name;
        ToolTip = _viewModel.ToolTip;
        InputOffRect.Fill = _viewModel.OffDetect ? Brushes.OrangeRed : Brushes.WhiteSmoke;
        InputOnRect.Fill = _viewModel.OnDetect ? Brushes.OrangeRed : Brushes.WhiteSmoke;
        ValueRect.Fill = _viewModel.CommandOn ? Brushes.LawnGreen : Brushes.WhiteSmoke;
    }

    private void ValueRect_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _viewModel.ToggleValue();
    }
}
