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
    private readonly IViewFactory _viewFactory;
    private readonly IActorRegistry _actorRegistry;
    private readonly Dictionary<string, (Button button, UserControl ioListView)> _buttons = [];

    public IoMonitorView(IViewFactory viewFactory, IActorRegistry actorRegistry)
    {
        InitializeComponent();

        _viewFactory = viewFactory;
        _actorRegistry = actorRegistry;

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

        var ioView = _viewFactory.Create<IoView>(button.Tag, 2)!;
        ioView.NameColumnWidth = 350;
        ioView.RowHeight = 30;

        _buttons[actorName] = (button, ioView)!;
        ActorButtonsPanel.Children.Add(button);
    }

    private void SelectInitialButton()
    {
        if (!_buttons.TryGetValue("Auxiliary", out var actorItem))
            return;

        _buttons["Auxiliary"].button.Content = "All";
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
