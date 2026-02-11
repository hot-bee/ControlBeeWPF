using System.ComponentModel;
using System.Runtime.CompilerServices;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;

namespace ControlBeeWPF.ViewModels;

public sealed class InPositionIndicatorViewModel : INotifyPropertyChanged
{
    private readonly Dictionary<AxisStatusViewModel, int> _axisStatusViewModels;
    private readonly (double current, double? target)[] _positions;

    private readonly double _tolerance;
    private readonly Dictionary<VariableViewModel, int> _variableViewModels;

    private Brush _backgroundBrush = Brushes.LawnGreen;

    public InPositionIndicatorViewModel(
        AxisStatusViewModel[] axisStatusViewModels,
        VariableViewModel[] variableViewModels,
        double tolerance
    )
    {
        var length = variableViewModels.Length;
        _tolerance = tolerance;

        _positions = new (double current, double? target)[length];
        _axisStatusViewModels = new Dictionary<AxisStatusViewModel, int>(length);
        _variableViewModels = new Dictionary<VariableViewModel, int>(length);

        for (var index = 0; index < length; index++)
        {
            var axisStatusViewModel = axisStatusViewModels[index];
            var variableViewModel = variableViewModels[index];

            _axisStatusViewModels[axisStatusViewModel] = index;
            _variableViewModels[variableViewModel] = index;

            axisStatusViewModel.PropertyChanged += AxisStatusViewModelOnPropertyChanged;
            variableViewModel.PropertyChanged += VariableViewModelOnPropertyChanged;

            UpdateCurrentPosition(index, axisStatusViewModel);
            UpdateTargetPosition(index, variableViewModel);
        }

        UpdateContent();
    }

    public Brush BackgroundBrush
    {
        get => _backgroundBrush;
        private set
        {
            if (Equals(_backgroundBrush, value))
                return;
            _backgroundBrush = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void AxisStatusViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(AxisStatusViewModel.CommandPosition))
            return;
        if (sender is not AxisStatusViewModel axisStatusViewModel)
            return;
        if (!_axisStatusViewModels.TryGetValue(axisStatusViewModel, out var index))
            return;

        UpdateCurrentPosition(index, axisStatusViewModel);
    }

    private void UpdateCurrentPosition(int index, AxisStatusViewModel axisStatusViewModel)
    {
        _positions[index].current = axisStatusViewModel.CommandPosition;
        UpdateContent();
    }

    private void VariableViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(VariableViewModel.Value))
            return;
        if (sender is not VariableViewModel variableViewModel)
            return;
        if (!_variableViewModels.TryGetValue(variableViewModel, out var index))
            return;
        UpdateTargetPosition(index, variableViewModel);
    }

    private void UpdateTargetPosition(int index, VariableViewModel variableViewModel)
    {
        if (variableViewModel.Value == null)
            return;
        _positions[index].target = (double)variableViewModel.Value;
        UpdateContent();
    }

    private void UpdateContent()
    {
        var isAligned = _positions.All(position =>
            position.target.HasValue
            && Math.Abs(position.current - position.target.Value) <= _tolerance
        );

        BackgroundBrush = isAligned ? Brushes.LawnGreen : Brushes.LightGray;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
