using System.Windows;
using ControlBee.Interfaces;
using ControlBeeWPF.Interfaces;
using Brushes = System.Windows.Media.Brushes;
using Button = System.Windows.Controls.Button;
using UserControl = System.Windows.Controls.UserControl;

namespace ControlBeeWPF.Views;

/// <summary>
///     Interaction logic for IoMonitorView.xaml
/// </summary>
public partial class IoMonitorView
{
    public static readonly DependencyProperty NameColumnWidthProperty = DependencyProperty.Register(
        nameof(NameColumnWidth),
        typeof(GridLength),
        typeof(IoMonitorView),
        new PropertyMetadata(new GridLength(7, GridUnitType.Star), OnNameColumnWidthChanged)
    );

    public static readonly DependencyProperty RowHeightProperty = DependencyProperty.Register(
        nameof(RowHeight),
        typeof(double),
        typeof(IoMonitorView),
        new PropertyMetadata(30.0, OnRowHeightChanged)
    );

    private static void OnNameColumnWidthChanged(
        DependencyObject dependencyObject,
        DependencyPropertyChangedEventArgs e
    )
    {
        if (dependencyObject is not IoMonitorView view)
            return;
        foreach (var (_, (_, ioListView)) in view._buttons)
        {
            if (ioListView is IoView ioView)
                ioView.NameColumnWidth = (GridLength)e.NewValue;
        }
    }

    private static void OnRowHeightChanged(
        DependencyObject dependencyObject,
        DependencyPropertyChangedEventArgs e
    )
    {
        if (dependencyObject is not IoMonitorView view)
            return;
        foreach (var (_, (_, ioListView)) in view._buttons)
        {
            if (ioListView is IoView ioView)
                ioView.RowHeight = (double)e.NewValue;
        }
    }

    public GridLength NameColumnWidth
    {
        get => (GridLength)GetValue(NameColumnWidthProperty);
        set => SetValue(NameColumnWidthProperty, value);
    }

    public double RowHeight
    {
        get => (double)GetValue(RowHeightProperty);
        set => SetValue(RowHeightProperty, value);
    }

    private readonly IViewFactory _viewFactory;
    private readonly IActorRegistry _actorRegistry;
    private readonly int _columns;
    private readonly string _initialActor;
    private readonly Dictionary<string, string>? _buttonRenames;
    private readonly Dictionary<string, (Button button, UserControl ioListView)> _buttons = [];

    public IoMonitorView(
        IViewFactory viewFactory,
        IActorRegistry actorRegistry,
        int? columns,
        string? initialActor,
        Dictionary<string, string>? buttonRenames
    )
    {
        InitializeComponent();

        _viewFactory = viewFactory;
        _actorRegistry = actorRegistry;
        _columns = columns ?? 2;
        _initialActor = initialActor ?? "Auxiliary";
        _buttonRenames = buttonRenames;

        BuildActorButtons();
        SelectInitialButton();
    }

    private (string actorName, string actorTitle)[] GetActorTitles() =>
        _actorRegistry
            .GetActorNames()
            .Where(actor => actor != "Ui")
            .Select(actor => (actor, _actorRegistry.Get(actor)!.Title))
            .ToArray();

    private void BuildActorButtons()
    {
        ActorButtonsPanel.Children.Clear();
        foreach (var (actorName, actorTitle) in GetActorTitles())
            AddButton(actorName, actorTitle);
    }

    private void AddButton(string actorName, string actorTitle)
    {
        var button = new Button
        {
            Content = actorTitle,
            Tag = actorName,
            Style = (Style)FindResource("ButtonStyle"),
        };
        button.Click += ActorButton_Click;

        var ioView = _viewFactory.Create<IoView>(button.Tag, _columns)!;
        ioView.NameColumnWidth = NameColumnWidth;
        ioView.RowHeight = RowHeight;

        _buttons[actorName] = (button, ioView)!;
        ActorButtonsPanel.Children.Add(button);
    }

    private void ApplyButtonRenames()
    {
        if (_buttonRenames == null)
            return;
        foreach (var (actorName, displayName) in _buttonRenames)
        {
            if (_buttons.TryGetValue(actorName, out var item))
                item.button.Content = displayName;
        }
    }

    private void SelectInitialButton()
    {
        ApplyButtonRenames();

        if (!_buttons.TryGetValue(_initialActor, out var actorItem))
            return;

        UpdateButtonColors(actorItem.button);
        IoListArea.Content = actorItem.ioListView;
    }

    private void ActorButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button)
            return;
        if (button.Tag is not string actorName)
            return;

        UpdateButtonColors(button);
        IoListArea.Content = _buttons[actorName].ioListView;
    }

    private void UpdateButtonColors(Button currentButton)
    {
        foreach (var actorItem in _buttons.Values)
            actorItem.button.ClearValue(BackgroundProperty);
        currentButton.Background = Brushes.LightGoldenrodYellow;
    }
}
