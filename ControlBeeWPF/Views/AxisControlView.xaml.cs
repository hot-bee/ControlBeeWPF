using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ControlBee.Constants;
using ControlBee.Interfaces;
using ControlBee.Models;
using ControlBee.Variables;
using ControlBeeAbstract.Constants;
using ControlBeeWPF.Interfaces;
using ControlBeeWPF.ViewModels;
using Button = System.Windows.Controls.Button;
using ComboBox = System.Windows.Controls.ComboBox;
using Dict = System.Collections.Generic.Dictionary<string, object?>;
using Message = ControlBee.Models.Message;
using RadioButton = System.Windows.Controls.RadioButton;

namespace ControlBeeWPF.Views;

/// <summary>
///     Interaction logic for AxisControlView.xaml
/// </summary>
public partial class AxisControlView
{
    private readonly IActorRegistry _actorRegistry;
    private readonly IViewFactory _viewFactory;
    private readonly IUiActor _ui;
    private readonly IActor _auxiliary;
    private readonly AxisControlViewModel _viewModel;
    private int _axisCount;
    private bool _axesInitDone;
    private double _jogStepCustom;
    private AxisDirection _currentAxisDirection;
    private JogSpeedLevel _currentJogSpeedLevel;
    private JogStep _currentJogStep;
    private JogType _currentJogType;

    private readonly Dictionary<(int axisIndex, string labelName), VariableItemView> _jogViews =
        new();
    private readonly Dictionary<
        (int axisIndex, string labelName),
        VariableItemView
    > _speedProfileViews = new();
    private Dictionary<int, (string ActorName, string ItemPath)>? _originalAxes;
    private SpeedProfileType _currentSpeedProfileType;
    private SpeedProfileType _repeatSpeedProfileType;
    private Func<object?, object?>? _accelDisplayConverter;
    private Func<object?, object?>? _accelInputConverter;
    private readonly bool _useJerkRatio;
    private readonly VariableViewModel _repeatStartPosition;
    private readonly VariableViewModel _repeatTargetPosition;

    public AxisControlView(
        IActorRegistry actorRegistry,
        IViewFactory viewFactory,
        AxisControlViewModel viewModel,
        string positionUnit,
        string velocityUnit,
        string accelDecelUnit,
        string? jerkUnit = null,
        bool useJerkRatio = false
    )
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = _viewModel;
        _viewModel.PropertyChanged += ViewModelOnPropertyChanged;

        _useJerkRatio = useJerkRatio;
        JerkLabel.Content = useJerkRatio ? "Jerk Ratio" : "Jerk";
        _viewModel.PositionUnit = positionUnit;
        _viewModel.VelocityUnit = velocityUnit;
        _viewModel.AccelDecelUnit = accelDecelUnit;
        _viewModel.JerkUnit = useJerkRatio ? "" : (jerkUnit ?? "");

        _actorRegistry = actorRegistry;
        _viewFactory = viewFactory;
        _ui = (UiActor)_actorRegistry.Get("Ui")!;
        _auxiliary = _actorRegistry.Get("Auxiliary")!;

        _ui.MessageArrived += UiActorOnMessageArrived;
        _auxiliary.Send(new Message(_ui, "_requestStatus"));

        StepRadioButton.IsChecked = true;
        SmallRadioButton.IsChecked = true;
        InitRadioButton.IsChecked = true;
        JogStepCustomArea.Content = _jogStepCustom;

        _repeatStartPosition = new VariableViewModel(
            _actorRegistry,
            "Auxiliary",
            "/RepeatStartPosition",
            (object[])[]
        );
        _repeatTargetPosition = new VariableViewModel(
            _actorRegistry,
            "Auxiliary",
            "/RepeatTargetPosition",
            (object[])[]
        );

        InitRepeatViews();
    }

    public void SetAccelDecelConverter(
        Func<object?, object?> displayConverter,
        Func<object?, object?> inputConverter
    )
    {
        _accelDisplayConverter = displayConverter;
        _accelInputConverter = inputConverter;
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(AxisControlViewModel.SelectedAxis))
            ApplySelectedAxisViews();
    }

    private void UiActorOnMessageArrived(object? sender, Message e)
    {
        switch (e.Name)
        {
            case "_status":
                if (e.Sender == _auxiliary)
                {
                    if (e.DictPayload?.GetValueOrDefault("AxisCount") is int axesCount)
                        if (!_axesInitDone)
                        {
                            _axisCount = axesCount;
                            _originalAxes =
                                e.DictPayload?.GetValueOrDefault("OriginalAxes")
                                as Dictionary<int, (string ActorName, string ItemPath)>;
                            AxesInit();
                        }

                    if (e.DictPayload?.GetValueOrDefault("OnRepeatMotion") is bool onRepeatMotion)
                        StartRepeatButton.IsEnabled = !onRepeatMotion;
                }

                break;
        }
    }

    private void AxesInit()
    {
        for (var index = 0; index < _axisCount; index++)
        {
            if (_originalAxes == null || !_originalAxes.TryGetValue(index, out var info))
                continue;

            var actorName = info.ActorName;
            var itemPath = $"/{info.ItemPath}";
            var axisStatusViewModel = new AxisStatusViewModel(_actorRegistry, actorName, itemPath);

            var jogStepSmallViewModel = new VariableViewModel(
                _actorRegistry,
                actorName,
                $"{itemPath}/StepJogSizes",
                (object[])[0]
            );
            var jogStepMediumViewModel = new VariableViewModel(
                _actorRegistry,
                actorName,
                $"{itemPath}/StepJogSizes",
                (object[])[1]
            );
            var jogStepLargeViewModel = new VariableViewModel(
                _actorRegistry,
                actorName,
                $"{itemPath}/StepJogSizes",
                (object[])[2]
            );

            var jogSpeedSlowViewModel = new VariableViewModel(
                _actorRegistry,
                actorName,
                $"{itemPath}/JogSpeedLevelFactors",
                (object[])[0]
            );
            var jogSpeedMediumViewModel = new VariableViewModel(
                _actorRegistry,
                actorName,
                $"{itemPath}/JogSpeedLevelFactors",
                (object[])[1]
            );
            var jogSpeedFastViewModel = new VariableViewModel(
                _actorRegistry,
                actorName,
                $"{itemPath}/JogSpeedLevelFactors",
                (object[])[2]
            );

            var initVelocityViewModel = new VariableViewModel(
                _actorRegistry,
                actorName,
                $"{itemPath}/InitSpeed",
                (object[])["Velocity"]
            );
            var initAccelViewModel = new VariableViewModel(
                _actorRegistry,
                actorName,
                $"{itemPath}/InitSpeed",
                (object[])["Accel"]
            );
            var initDecelViewModel = new VariableViewModel(
                _actorRegistry,
                actorName,
                $"{itemPath}/InitSpeed",
                (object[])["Decel"]
            );
            var initAccelJerkViewModel = new VariableViewModel(
                _actorRegistry,
                actorName,
                $"{itemPath}/InitSpeed",
                (object[])["AccelJerkRatio"]
            );
            var initDecelJerkViewModel = new VariableViewModel(
                _actorRegistry,
                actorName,
                $"{itemPath}/InitSpeed",
                (object[])["DecelJerkRatio"]
            );

            var jogVelocityViewModel = new VariableViewModel(
                _actorRegistry,
                actorName,
                $"{itemPath}/JogSpeed",
                (object[])["Velocity"]
            );
            var jogAccelViewModel = new VariableViewModel(
                _actorRegistry,
                actorName,
                $"{itemPath}/JogSpeed",
                (object[])["Accel"]
            );
            var jogDecelViewModel = new VariableViewModel(
                _actorRegistry,
                actorName,
                $"{itemPath}/JogSpeed",
                (object[])["Decel"]
            );
            var jogAccelJerkViewModel = new VariableViewModel(
                _actorRegistry,
                actorName,
                $"{itemPath}/JogSpeed",
                (object[])["AccelJerkRatio"]
            );
            var jogDecelJerkViewModel = new VariableViewModel(
                _actorRegistry,
                actorName,
                $"{itemPath}/JogSpeed",
                (object[])["DecelJerkRatio"]
            );

            var normalVelocityViewModel = new VariableViewModel(
                _actorRegistry,
                actorName,
                $"{itemPath}/NormalSpeed",
                (object[])["Velocity"]
            );
            var normalAccelViewModel = new VariableViewModel(
                _actorRegistry,
                actorName,
                $"{itemPath}/NormalSpeed",
                (object[])["Accel"]
            );
            var normalDecelViewModel = new VariableViewModel(
                _actorRegistry,
                actorName,
                $"{itemPath}/NormalSpeed",
                (object[])["Decel"]
            );
            var normalAccelJerkViewModel = new VariableViewModel(
                _actorRegistry,
                actorName,
                $"{itemPath}/NormalSpeed",
                (object[])["AccelJerkRatio"]
            );
            var normalDecelJerkViewModel = new VariableViewModel(
                _actorRegistry,
                actorName,
                $"{itemPath}/NormalSpeed",
                (object[])["DecelJerkRatio"]
            );

            var axisInfo = new AxisControlViewModel.AxisInfo(
                index,
                axisStatusViewModel,
                actorName,
                itemPath,
                jogStepSmallViewModel,
                jogStepMediumViewModel,
                jogStepLargeViewModel,
                jogSpeedSlowViewModel,
                jogSpeedMediumViewModel,
                jogSpeedFastViewModel,
                initVelocityViewModel,
                initAccelViewModel,
                initDecelViewModel,
                initAccelJerkViewModel,
                initDecelJerkViewModel,
                jogVelocityViewModel,
                jogAccelViewModel,
                jogDecelViewModel,
                jogAccelJerkViewModel,
                jogDecelJerkViewModel,
                normalVelocityViewModel,
                normalAccelViewModel,
                normalDecelViewModel,
                normalAccelJerkViewModel,
                normalDecelJerkViewModel
            );
            _viewModel.Axes.Add(axisInfo);

            _jogViews[(index, "StepSmall")] = CreateStretchVariableItemView(jogStepSmallViewModel);
            _jogViews[(index, "StepMedium")] = CreateStretchVariableItemView(
                jogStepMediumViewModel
            );
            _jogViews[(index, "StepLarge")] = CreateStretchVariableItemView(jogStepLargeViewModel);
            _jogViews[(index, "SpeedSlow")] = CreateStretchVariableItemView(jogSpeedSlowViewModel);
            _jogViews[(index, "SpeedMedium")] = CreateStretchVariableItemView(
                jogSpeedMediumViewModel
            );
            _jogViews[(index, "SpeedFast")] = CreateStretchVariableItemView(jogSpeedFastViewModel);

            var centerCorner = new CornerRadius(0);
            var centerMargin = new Thickness(-1, 0, -1, 0);

            _speedProfileViews[(index, "InitVelocity")] = CreateStretchVariableItemView(
                initVelocityViewModel,
                cornerRadius: centerCorner,
                margin: centerMargin
            );
            _speedProfileViews[(index, "InitAccel")] = CreateStretchVariableItemView(
                initAccelViewModel,
                _accelDisplayConverter,
                _accelInputConverter,
                centerCorner,
                centerMargin
            );
            _speedProfileViews[(index, "JogVelocity")] = CreateStretchVariableItemView(
                jogVelocityViewModel,
                cornerRadius: centerCorner,
                margin: centerMargin
            );
            _speedProfileViews[(index, "JogAccel")] = CreateStretchVariableItemView(
                jogAccelViewModel,
                _accelDisplayConverter,
                _accelInputConverter,
                centerCorner,
                centerMargin
            );
            _speedProfileViews[(index, "NormalVelocity")] = CreateStretchVariableItemView(
                normalVelocityViewModel,
                cornerRadius: centerCorner,
                margin: centerMargin
            );
            _speedProfileViews[(index, "NormalAccel")] = CreateStretchVariableItemView(
                normalAccelViewModel,
                _accelDisplayConverter,
                _accelInputConverter,
                centerCorner,
                centerMargin
            );

            if (_useJerkRatio)
            {
                _speedProfileViews[(index, "InitJerk")] = CreateStretchVariableItemView(
                    initAccelJerkViewModel,
                    cornerRadius: centerCorner,
                    margin: centerMargin
                );
                _speedProfileViews[(index, "JogJerk")] = CreateStretchVariableItemView(
                    jogAccelJerkViewModel,
                    cornerRadius: centerCorner,
                    margin: centerMargin
                );
                _speedProfileViews[(index, "NormalJerk")] = CreateStretchVariableItemView(
                    normalAccelJerkViewModel,
                    cornerRadius: centerCorner,
                    margin: centerMargin
                );
            }
            else
            {
                var (initJerkDisplay, initJerkInput) = CreateJerkConverters(
                    initVelocityViewModel,
                    initAccelViewModel
                );
                _speedProfileViews[(index, "InitJerk")] = CreateStretchVariableItemView(
                    initAccelJerkViewModel,
                    initJerkDisplay,
                    initJerkInput,
                    centerCorner,
                    centerMargin
                );

                var (jogJerkDisplay, jogJerkInput) = CreateJerkConverters(
                    jogVelocityViewModel,
                    jogAccelViewModel
                );
                _speedProfileViews[(index, "JogJerk")] = CreateStretchVariableItemView(
                    jogAccelJerkViewModel,
                    jogJerkDisplay,
                    jogJerkInput,
                    centerCorner,
                    centerMargin
                );

                var (normalJerkDisplay, normalJerkInput) = CreateJerkConverters(
                    normalVelocityViewModel,
                    normalAccelViewModel
                );
                _speedProfileViews[(index, "NormalJerk")] = CreateStretchVariableItemView(
                    normalAccelJerkViewModel,
                    normalJerkDisplay,
                    normalJerkInput,
                    centerCorner,
                    centerMargin
                );

                SubscribeJerkRefresh(index, "Init", initVelocityViewModel, initAccelViewModel);
                SubscribeJerkRefresh(index, "Jog", jogVelocityViewModel, jogAccelViewModel);
                SubscribeJerkRefresh(
                    index,
                    "Normal",
                    normalVelocityViewModel,
                    normalAccelViewModel
                );
            }
        }

        _viewModel.SelectedAxis = _viewModel.Axes[0];
        ApplySelectedAxisViews();
        _axesInitDone = true;
    }

    private void ApplySelectedAxisViews()
    {
        var axis = _viewModel.SelectedAxis;
        if (axis is null)
            return;

        var axisIndex = axis.Index;

        JogStepSmallArea.Content = _jogViews.GetValueOrDefault((axisIndex, "StepSmall"));
        JogStepMediumArea.Content = _jogViews.GetValueOrDefault((axisIndex, "StepMedium"));
        JogStepLargeArea.Content = _jogViews.GetValueOrDefault((axisIndex, "StepLarge"));
        JogSpeedSlowArea.Content = _jogViews.GetValueOrDefault((axisIndex, "SpeedSlow"));
        JogSpeedMediumArea.Content = _jogViews.GetValueOrDefault((axisIndex, "SpeedMedium"));
        JogSpeedFastArea.Content = _jogViews.GetValueOrDefault((axisIndex, "SpeedFast"));

        ApplySpeedProfileViews();
    }

    private void ApplySpeedProfileViews()
    {
        var axis = _viewModel.SelectedAxis;
        if (axis is null)
            return;

        var axisIndex = axis.Index;
        var speedProfileType = _currentSpeedProfileType switch
        {
            SpeedProfileType.Init => "Init",
            SpeedProfileType.Jog => "Jog",
            SpeedProfileType.Normal => "Normal",
            _ => "Init",
        };

        VelocityArea.Content = _speedProfileViews.GetValueOrDefault(
            (axisIndex, $"{speedProfileType}Velocity")
        );
        AccelDecelArea.Content = _speedProfileViews.GetValueOrDefault(
            (axisIndex, $"{speedProfileType}Accel")
        );
        JerkArea.Content = _speedProfileViews.GetValueOrDefault(
            (axisIndex, $"{speedProfileType}Jerk")
        );
    }

    private static (
        Func<object?, object?> display,
        Func<object?, object?> input
    ) CreateJerkConverters(VariableViewModel? velocityViewModel, VariableViewModel? accelViewModel)
    {
        return (Display, Input);

        object? Input(object? value)
        {
            var jerk = value as double? ?? 0;
            var velocity = velocityViewModel?.Value as double? ?? 0;
            var accel = accelViewModel?.Value as double? ?? 0;
            if (jerk == 0 || velocity == 0)
                return 0.0;
            return accel * accel / (jerk * velocity);
        }

        object? Display(object? value)
        {
            var ratio = value as double? ?? 0;
            var velocity = velocityViewModel?.Value as double? ?? 0;
            var accel = accelViewModel?.Value as double? ?? 0;
            if (velocity == 0 || ratio == 0)
                return 0.0;
            return accel * accel / (ratio * velocity);
        }
    }

    private void SubscribeJerkRefresh(
        int axisIndex,
        string speedProfileType,
        VariableViewModel? velocityViewModel,
        VariableViewModel? accelViewModel
    )
    {
        if (velocityViewModel == null || accelViewModel == null)
            return;
        velocityViewModel.PropertyChanged += Handler;
        accelViewModel.PropertyChanged += Handler;
        return;

        void Handler(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(VariableViewModel.Value))
                return;
            if (
                _speedProfileViews.TryGetValue((axisIndex, $"{speedProfileType}Jerk"), out var view)
            )
                view.Refresh();
        }
    }

    private VariableItemView CreateStretchVariableItemView(
        VariableViewModel viewModel,
        Func<object?, object?>? displayConverter = null,
        Func<object?, object?>? inputConverter = null,
        CornerRadius? cornerRadius = null,
        Thickness? margin = null
    )
    {
        var view = _viewFactory.Create<VariableItemView>(viewModel)!;
        view.ItemWidth = double.NaN;
        view.DisplayConverter = displayConverter;
        view.InputConverter = inputConverter;
        if (cornerRadius != null)
            view.CornerRadius = cornerRadius.Value;
        if (margin != null)
            view.Margin = margin.Value;
        return view;
    }

    private void SpeedProfileRadioButton_OnChecked(object sender, RoutedEventArgs e)
    {
        if (sender is not RadioButton radioButton)
            return;
        if (radioButton.Tag is not string tag)
            return;

        _currentSpeedProfileType = tag switch
        {
            "Init" => SpeedProfileType.Init,
            "Jog" => SpeedProfileType.Jog,
            "Normal" => SpeedProfileType.Normal,
            _ => _currentSpeedProfileType,
        };

        ApplySpeedProfileViews();
    }

    private void JogRadioButton_OnChecked(object sender, RoutedEventArgs e)
    {
        if (sender is not RadioButton radioButton)
            return;

        if (radioButton.Tag is not string tag)
            return;

        _currentJogType = tag switch
        {
            "Step" => JogType.Step,
            "Continuous" => JogType.Continuous,
            _ => _currentJogType,
        };

        StepJogPanel.Visibility =
            _currentJogType == JogType.Step ? Visibility.Visible : Visibility.Collapsed;
        ContinuousJogPanel.Visibility =
            _currentJogType == JogType.Continuous ? Visibility.Visible : Visibility.Collapsed;
    }

    private void JogStepButton_OnChecked(object sender, RoutedEventArgs e)
    {
        if (sender is not RadioButton radioButton)
            return;

        _currentJogStep = radioButton.Tag!.ToString() switch
        {
            "Small" => JogStep.Small,
            "Medium" => JogStep.Medium,
            "Large" => JogStep.Large,
            "Custom" => JogStep.Custom,
            _ => _currentJogStep,
        };
    }

    private void JogSpeedButton_OnChecked(object sender, RoutedEventArgs e)
    {
        if (sender is not RadioButton radioButton)
            return;

        _currentJogSpeedLevel = radioButton.Tag!.ToString() switch
        {
            "Slow" => JogSpeedLevel.Slow,
            "Medium" => JogSpeedLevel.Medium,
            "Fast" => JogSpeedLevel.Fast,
            _ => _currentJogSpeedLevel,
        };
    }

    private void SendJogStartMessage(Button button, bool isRightClick)
    {
        _currentAxisDirection = button switch
        {
            not null when button == LeftButton || button == DownButton => AxisDirection.Negative,

            not null when button == RightButton || button == UpButton => AxisDirection.Positive,

            _ => throw new ArgumentOutOfRangeException(nameof(button)),
        };

        if (isRightClick)
            _currentAxisDirection = (AxisDirection)((int)_currentAxisDirection * -1);

        var selectedAxis = _viewModel.SelectedAxis!;
        var actor = _actorRegistry.Get(selectedAxis.ActorName!)!;

        switch (_currentJogType)
        {
            case JogType.Step when _currentJogStep == JogStep.Custom:
                actor.Send(
                    new ActorItemMessage(
                        _ui,
                        selectedAxis.ItemPath,
                        "_jogStart",
                        new Dict
                        {
                            ["Type"] = "Step",
                            ["Direction"] = _currentAxisDirection,
                            ["JogStep"] = JogStep.Custom,
                            ["CustomStepSize"] = _jogStepCustom,
                        }
                    )
                );
                break;
            case JogType.Step:
                actor.Send(
                    new ActorItemMessage(
                        _ui,
                        selectedAxis.ItemPath,
                        "_jogStart",
                        new Dict
                        {
                            ["Type"] = "Step",
                            ["Direction"] = _currentAxisDirection,
                            ["JogStep"] = _currentJogStep,
                        }
                    )
                );
                break;
            case JogType.Continuous:
                actor.Send(
                    new ActorItemMessage(
                        _ui,
                        selectedAxis.ItemPath,
                        "_jogStart",
                        new Dict
                        {
                            ["Type"] = "Continuous",
                            ["Direction"] = _currentAxisDirection,
                            ["JogSpeed"] = _currentJogSpeedLevel,
                        }
                    )
                );
                break;
        }
    }

    private void JogButton_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        SendJogStartMessage((Button)sender, false);
    }

    private void JogButton_OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        SendJogStartMessage((Button)sender, true);
    }

    private void JogButton_OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        var selectedAxis = _viewModel.SelectedAxis!;
        var actor = _actorRegistry.Get(selectedAxis.ActorName!)!;
        actor.Send(new ActorItemMessage(_ui, selectedAxis.ItemPath, "_jogStop"));
    }

    private void InitRepeatViews()
    {
        var centerCorner = new CornerRadius(0);
        var centerMargin = new Thickness(-1, 0, -1, 0);
        var rearCorner = new CornerRadius(0, 6, 6, 0);
        var rearMargin = new Thickness(-1, 0, 0, 0);

        RepeatCountArea.Content = CreateStretchVariableItemView(
            new VariableViewModel(_actorRegistry, "Auxiliary", "/RepeatCount", (object[])[]),
            cornerRadius: rearCorner,
            margin: rearMargin
        );
        DelayTimeArea.Content = CreateStretchVariableItemView(
            new VariableViewModel(_actorRegistry, "Auxiliary", "/RepeatDelayTime", (object[])[]),
            cornerRadius: centerCorner,
            margin: centerMargin
        );
        RepeatStartPositionArea.Content = CreateStretchVariableItemView(
            _repeatStartPosition,
            cornerRadius: centerCorner,
            margin: centerMargin
        );
        RepeatTargetPositionArea.Content = CreateStretchVariableItemView(
            _repeatTargetPosition,
            cornerRadius: centerCorner,
            margin: centerMargin
        );
    }

    private void RepeatSpeedProfileComboBox_OnSelectionChanged(
        object sender,
        SelectionChangedEventArgs e
    )
    {
        if (sender is not ComboBox comboBox)
            return;
        _repeatSpeedProfileType = comboBox.SelectedIndex switch
        {
            0 => SpeedProfileType.Init,
            1 => SpeedProfileType.Jog,
            2 => SpeedProfileType.Normal,
            _ => _repeatSpeedProfileType,
        };
    }

    // TODO: Change to perform motion repeat via message
    private void StartRepeatButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (_viewModel.SelectedAxis is null)
            return;
        var selectedAxis = _viewModel.SelectedAxis;
        var originalActor = _actorRegistry.Get(selectedAxis.ActorName)!;
        var originalAxis = (IAxis)originalActor.GetItem(selectedAxis.ItemPath)!;
        var speed = _repeatSpeedProfileType switch
        {
            SpeedProfileType.Init => (SpeedProfile)originalAxis.GetInitSpeed().Clone(),
            SpeedProfileType.Jog => (SpeedProfile)
                originalAxis.GetJogSpeed(JogSpeedLevel.Fast).Clone(),
            SpeedProfileType.Normal => (SpeedProfile)originalAxis.GetNormalSpeed().Clone(),
            _ => (SpeedProfile)originalAxis.GetInitSpeed().Clone(),
        };
        _auxiliary.Send(
            new Message(
                _ui,
                "StartRepeatMotion",
                new Dict { ["Axis"] = originalAxis, ["SpeedProfile"] = speed }
            )
        );
    }

    private void StopRepeatButton_OnClick(object sender, RoutedEventArgs e)
    {
        _auxiliary.Send(new Message(_ui, "StopRepeatMotion"));
    }

    private void JogStepCustomArea_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var numpad = _viewFactory.Create<NumpadView>(_jogStepCustom.ToString(), true);
        if (numpad!.ShowDialog() is not true)
            return;
        if (!double.TryParse(numpad.Value.Replace(",", ""), out var value))
            return;
        _jogStepCustom = value;
        JogStepCustomArea.Content = _jogStepCustom;
    }

    private void SetButton_OnClick(object sender, RoutedEventArgs e)
    {
        var axis = _viewModel.SelectedAxis;
        if (axis is null)
            return;

        if (sender.Equals(SetStartPosButton))
        {
            _repeatStartPosition.ChangeValue($"{axis.CommandPos}");
        }
        else if (sender.Equals(SetTargetPosButton))
        {
            _repeatTargetPosition.ChangeValue($"{axis.CommandPos}");
        }
    }

    private enum JogType
    {
        Step,
        Continuous,
    }

    private enum SpeedProfileType
    {
        Init,
        Jog,
        Normal,
    }
}
