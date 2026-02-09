using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ControlBeeWPF.ViewModels;

public sealed class InPositionIndicatorViewModel : INotifyPropertyChanged
{
    private readonly Dictionary<AxisStatusViewModel, int> _axisStatusViewModels;
    private readonly Dictionary<VariableViewModel, int> _variableViewModels;
    private readonly (double current, double target)[] _positions;
    private readonly double _tolerance;

    private Brush _backgroundBrush = Brushes.LawnGreen;
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

    public InPositionIndicatorViewModel(
        AxisStatusViewModel[] axisStatusViewModels,
        VariableViewModel[] variableViewModels,
        double tolerance
    )
    {
        var length = variableViewModels.Length;
        _tolerance = tolerance;

        _positions = new (double current, double target)[length];
        _axisStatusViewModels = new Dictionary<AxisStatusViewModel, int>(length);
        _variableViewModels = new Dictionary<VariableViewModel, int>(length);

        for (var i = 0; i < length; i++)
        {
            var axisStatusViewModel = axisStatusViewModels[i];
            var variableViewModel = variableViewModels[i];

            _axisStatusViewModels[axisStatusViewModel] = i;
            _variableViewModels[variableViewModel] = i;

            axisStatusViewModel.PropertyChanged += AxisStatusViewModelOnPropertyChanged;
            variableViewModel.PropertyChanged += VariableViewModelOnPropertyChanged;
        }

        UpdateContent();
    }

    private void AxisStatusViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(AxisStatusViewModel.CommandPosition))
            return;
        if (sender is not AxisStatusViewModel axisStatusViewModel)
            return;
        if (!_axisStatusViewModels.TryGetValue(axisStatusViewModel, out var index))
            return;

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

        _positions[index].target = (double)variableViewModel.Value!;
        UpdateContent();
    }

    private void UpdateContent()
    {
        var isAligned = _positions.All(position =>
            Math.Abs(position.current - position.target) <= _tolerance
        );

        BackgroundBrush = isAligned ? Brushes.LawnGreen : Brushes.LightGray;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
