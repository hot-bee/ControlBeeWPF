using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using ControlBee.Constants;
using ControlBee.Interfaces;
using ControlBeeAbstract.Exceptions;
using ControlBeeWPF.Components;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using InitializationViewModel = ControlBeeWPF.ViewModels.InitializationViewModel;
using Message = ControlBee.Models.Message;
using ProgressBar = System.Windows.Controls.ProgressBar;

namespace ControlBeeWPF.Views;

/// <summary>
///     Interaction logic for InitializationView.xaml
/// </summary>
public partial class InitializationView
{
    private readonly Dictionary<string, ToggleImageButton> _buttonMap = new();
    private readonly Dictionary<string, ProgressBar> _progressMap = new();
    private readonly InitializationViewModel _viewModel;

    public InitializationView(IActorRegistry actorRegistry, InitializationViewModel viewModel)
    {
        _viewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();
        MakeButtons();
        UpdateButtonColor();
        _viewModel.PropertyChanged += _viewModel_PropertyChanged;

        var uiActor = (IUiActor)actorRegistry.Get("Ui")!;
        uiActor.MessageArrived += UiActorOnMessageArrived;
    }

    private void UiActorOnMessageArrived(object? sender, Message message)
    {
        switch (message.Name)
        {
            case "initializeProgress":
                {
                    var actorName = message.ActorName;
                    var value = (int)message.Payload!;

                    UpdateInitializeProgress(actorName, value);

                    break;
                }
        }
    }

    private void UpdateInitializeProgress(string actorName, int value)
    {
        if (!_progressMap.TryGetValue(actorName, out var progressBar))
            return;

        value = Math.Max(0, Math.Min(100, value));

        void ApplyProgress()
        {
            var anim = new DoubleAnimation
            {
                To = value,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut },
            };

            progressBar.BeginAnimation(
                RangeBase.ValueProperty,
                anim,
                HandoffBehavior.SnapshotAndReplace
            );
        }

        ApplyProgress();
    }

    public void Dispose()
    {
        _viewModel.PropertyChanged -= _viewModel_PropertyChanged;
        foreach (var (_, button) in _buttonMap)
            button.PropertyChanged -= CheckButton_PropertyChanged;
    }

    private void MakeButtons()
    {
        foreach (var (actorName, actorTitle) in _viewModel.GetActorTitles())
        {
            var checkButton = new ToggleImageButton(
                new BitmapImage(
                    new Uri("/Images/326558_blank_check_box_icon.png", UriKind.RelativeOrAbsolute)
                ),
                new BitmapImage(
                    new Uri("/Images/326561_box_check_icon.png", UriKind.RelativeOrAbsolute)
                ),
                actorTitle
            )
            {
                Width = 150,
                Height = 60,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(10, 0, 0, 0),
                Margin = new Thickness(10),
            };

            var progressBar = new ProgressBar
            {
                Width = 150,
                Height = 15,
                Margin = new Thickness(0, 5, 0, 0),
                Minimum = 0,
                Maximum = 100,
                Value = 0,
            };

            var container = new StackPanel { Margin = new Thickness(10) };

            container.Children.Add(checkButton);
            container.Children.Add(progressBar);

            _buttonMap[actorName] = checkButton;
            _progressMap[actorName] = progressBar;
            WrapPanel1.Children.Add(container);

            checkButton.PropertyChanged += CheckButton_PropertyChanged;
            checkButton.IsChecked = true;
        }
    }

    private void CheckButton_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "IsChecked")
        {
            var button = (ToggleImageButton)sender!;
            var actorName = _buttonMap.FirstOrDefault(x => x.Value == button).Key;
            _viewModel.SetInitialization(actorName, button.IsChecked);
        }
    }

    private void _viewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(_viewModel.InitializationStatus):
                {
                    UpdateButtonColor();
                    break;
                }
        }
    }

    private void UpdateButtonColor()
    {
        foreach (var (actorName, initialized) in _viewModel.InitializationStatus)
        {
            switch (initialized)
            {
                case InitializationStatus.Uninitialized:
                    _buttonMap[actorName].ClearValue(BackgroundProperty);
                    break;
                case InitializationStatus.Initialized:
                    _buttonMap[actorName].Background = new SolidColorBrush(Colors.LightGreen);
                    break;
                case InitializationStatus.Initializing:
                    _buttonMap[actorName].Background = new SolidColorBrush(Colors.Yellow);
                    break;
                case InitializationStatus.Error:
                    _buttonMap[actorName].Background = new SolidColorBrush(Colors.SlateGray);
                    break;
                default:
                    throw new ValueError();
            }
        }

        if (_viewModel.InitializationAll)
            InitializeAllButton.Background = new SolidColorBrush(Colors.LightGreen);
        else
            InitializeAllButton.ClearValue(BackgroundProperty);
    }

    private void SelectAllButton_OnClick(object sender, RoutedEventArgs e)
    {
        foreach (var (_, button) in _buttonMap)
            button.IsChecked = true;
    }

    private void DeselectAllButton_OnClick(object sender, RoutedEventArgs e)
    {
        foreach (var (_, button) in _buttonMap)
            button.IsChecked = false;
    }
}