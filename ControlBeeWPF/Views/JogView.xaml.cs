using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using ControlBee.Constants;
using ControlBeeAbstract.Exceptions;
using ControlBeeWPF.ViewModels;
using ControlBeeAbstract.Constants;
using log4net;
using Button = System.Windows.Controls.Button;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Label = System.Windows.Controls.Label;
using Orientation = System.Windows.Controls.Orientation;
using RadioButton = System.Windows.Controls.RadioButton;
using Rectangle = System.Windows.Shapes.Rectangle;
using UserControl = System.Windows.Controls.UserControl;

namespace ControlBeeWPF.Views;

/// <summary>
///     Interaction logic for TeachingJogView.xaml
/// </summary>
public partial class JogView : UserControl, IDisposable
{
    private static readonly ILog Logger = LogManager.GetLogger(
        MethodBase.GetCurrentMethod()!.DeclaringType!
    );
    private readonly Dictionary<Button, string> _jogButtons = new();
    private readonly JogViewModel _viewModel;

    public JogView(JogViewModel viewModel)
    {
        _viewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();

        viewModel.Loaded += ViewModelOnLoaded;
    }

    private void ViewModelOnLoaded(object? sender, EventArgs e)
    {
        ContinuousAxesContent.Content = UpdateAxesButtons(JogMode.Continuous);
        DiscreteAxesContent.Content = UpdateAxesButtons(JogMode.Step);
        SetupDiscreteStepOptions();
    }

    public void Dispose()
    {
        foreach (var button in _jogButtons.Keys)
        {
            button.MouseLeftButtonDown -= ContNegButtonOnMouseLeftButtonDown;
            button.MouseLeftButtonDown -= ContPosButtonOnMouseLeftButtonDown;
            button.MouseLeftButtonUp -= ContButtonOnMouseLeftButtonUp;
            button.MouseRightButtonDown -= ContPosButtonOnMouseLeftButtonDown;
            button.MouseRightButtonDown -= ContNegButtonOnMouseLeftButtonDown;
            button.MouseRightButtonUp -= ContButtonOnMouseLeftButtonUp;
        }
    }

    private void SetupDiscreteStepOptions()
    {
        DiscreteStepOptions.Children.Clear();
        var stepSizes = (string[])["Small", "Medium", "Large"];
        foreach (var step in stepSizes)
        {
            if (DiscreteStepOptions.Children.Count > 0)
                DiscreteStepOptions.Children.Add(new Rectangle { Width = 60 });
            var radioButton = new RadioButton
            {
                Content = $"{step}",
                VerticalContentAlignment = VerticalAlignment.Center,
            };
            radioButton.Checked += RadioButtonOnChecked;
            _stepRadios.Add(radioButton);
            DiscreteStepOptions.Children.Add(radioButton);
        }

        _stepRadios[1].IsChecked = true;
    }

    private void RadioButtonOnChecked(object sender, RoutedEventArgs e)
    {
        var stepIndex = _stepRadios.IndexOf((RadioButton)sender);
        foreach (var (itemPath, label) in _axisLabels)
        {
            var step = _viewModel.StepJogSizes[itemPath][stepIndex];
            label.Content = $"{itemPath}\n({step})";
        }
    }

    private readonly Dictionary<string, Label> _axisLabels = [];
    private readonly List<RadioButton> _stepRadios = [];

    private UIElement UpdateAxesButtons(JogMode jogMode)
    {
        var stackPanel = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
        foreach (var itemPath in _viewModel.AxisItemPaths)
        {
            var negButton = new Button
            {
                Content = "- Neg",
                Margin = new Thickness(10),
            };
            var posButton = new Button
            {
                Content = "Pos +",
                Margin = new Thickness(10),
            };
            var label = new Label
            {
                Content = itemPath,
                Width = 80,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
            };
            _axisLabels[itemPath] = label;
            stackPanel.Children.Add(
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Children = { negButton, label, posButton },
                }
            );
            switch (jogMode)
            {
                case JogMode.Continuous:
                    posButton.PreviewMouseLeftButtonDown += ContPosButtonOnMouseLeftButtonDown;
                    posButton.PreviewMouseLeftButtonUp += ContButtonOnMouseLeftButtonUp;
                    posButton.PreviewMouseRightButtonDown += ContNegButtonOnMouseLeftButtonDown;
                    posButton.PreviewMouseRightButtonUp += ContButtonOnMouseLeftButtonUp;

                    negButton.PreviewMouseLeftButtonDown += ContNegButtonOnMouseLeftButtonDown;
                    negButton.PreviewMouseLeftButtonUp += ContButtonOnMouseLeftButtonUp;
                    negButton.PreviewMouseRightButtonDown += ContPosButtonOnMouseLeftButtonDown;
                    negButton.PreviewMouseRightButtonUp += ContButtonOnMouseLeftButtonUp;
                    break;
                case JogMode.Step:
                    negButton.PreviewMouseLeftButtonDown += StepNegButtonOnMouseLeftButtonDown;
                    negButton.PreviewMouseRightButtonDown += StepPosButtonOnMouseLeftButtonDown;

                    posButton.PreviewMouseLeftButtonDown += StepPosButtonOnMouseLeftButtonDown;
                    posButton.PreviewMouseRightButtonDown += StepNegButtonOnMouseLeftButtonDown;
                    break;
            }
            _jogButtons[negButton] = itemPath;
            _jogButtons[posButton] = itemPath;
        }

        return stackPanel;
    }

    private void StepNegButtonOnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        Logger.Debug("Left button down on negative button.");
        var itemPath = _jogButtons[(Button)sender];
        _viewModel.StepMoveStart(itemPath, AxisDirection.Negative, GetStepMoveLevel());
    }

    private void StepPosButtonOnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        Logger.Debug("Left button down on positive button.");
        var itemPath = _jogButtons[(Button)sender];
        _viewModel.StepMoveStart(itemPath, AxisDirection.Positive, GetStepMoveLevel());
    }

    private void ContNegButtonOnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        Logger.Debug("Left button down on negative button.");
        var itemPath = _jogButtons[(Button)sender];
        _viewModel.ContinuousMoveStart(
            itemPath,
            AxisDirection.Negative,
            GetContinuousMoveSpeedLevel()
        );
    }

    private void ContPosButtonOnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        Logger.Debug("Left button down on positive button.");
        var itemPath = _jogButtons[(Button)sender];
        _viewModel.ContinuousMoveStart(
            itemPath,
            AxisDirection.Positive,
            GetContinuousMoveSpeedLevel()
        );
    }

    private void ContButtonOnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        Logger.Debug("Left Button Up.");
        var itemPath = _jogButtons[(Button)sender];
        _viewModel.ContinuousMoveStop(itemPath);
    }

    private JogSpeedLevel GetContinuousMoveSpeedLevel()
    {
        if (SpeedLowRadio.IsChecked is true)
            return JogSpeedLevel.Slow;
        if (SpeedMidRadio.IsChecked is true)
            return JogSpeedLevel.Medium;
        if (SpeedHighRadio.IsChecked is true)
            return JogSpeedLevel.Fast;
        throw new ValueError();
    }

    private JogStep GetStepMoveLevel()
    {
        if (_stepRadios[0].IsChecked is true)
            return JogStep.Small;
        if (_stepRadios[1].IsChecked is true)
            return JogStep.Medium;
        if (_stepRadios[2].IsChecked is true)
            return JogStep.Large;
        throw new ValueError();
    }

    private enum JogMode
    {
        Continuous,
        Step,
    }
}
