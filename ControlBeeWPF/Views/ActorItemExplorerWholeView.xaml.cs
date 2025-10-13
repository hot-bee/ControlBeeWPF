using System.Windows;
using ControlBee.Interfaces;
using ControlBeeWPF.Interfaces;
using ControlBeeWPF.Services;
using Brushes = System.Windows.Media.Brushes;
using Button = System.Windows.Controls.Button;
using UserControl = System.Windows.Controls.UserControl;

namespace ControlBeeWPF.Views;

public partial class ActorItemExplorerWholeView : UserControl, IDisposable
{
    private readonly ActorItemExplorerViewFactory _actorItemExplorerViewFactory;
    private readonly IActorRegistry _actorRegistry;

    private readonly Dictionary<string, Button> _buttons = new();
    private readonly IViewFactory _viewFactory;

    private readonly Dictionary<string, UserControl> _views = new();

    public ActorItemExplorerWholeView(
        IViewFactory viewFactory,
        IActorRegistry actorRegistry,
        ActorItemExplorerViewFactory actorItemExplorerViewFactory
    )
    {
        _viewFactory = viewFactory;
        _actorRegistry = actorRegistry;
        _actorItemExplorerViewFactory = actorItemExplorerViewFactory;
        InitializeComponent();

        var nameTitleParis = GetActorNameTitlePairs();

        foreach (var (name, title) in nameTitleParis)
        {
            var button = new Button
            {
                Content = title,
                Height = 40,
                Margin = new Thickness(1)
            };
            button.Click += (sender, args) => { SelectActor(name); };
            _buttons[name] = button;
            ActorPanel.Children.Add(button);
        }

        SelectActor(nameTitleParis[0].name);
    }

    public void Dispose()
    {
        foreach (var view in _views.Values)
            if (view is IDisposable disposable)
                disposable.Dispose();
    }

    public void ClearButtonColor()
    {
        foreach (var button in _buttons.Values)
            button.ClearValue(BackgroundProperty);
    }

    private IActor[] GetActors()
    {
        return _actorRegistry.GetActors().Where(x => x.GetItems().Length > 0).ToArray();
    }

    public (string name, string Title)[] GetActorNameTitlePairs()
    {
        return GetActors().Select(x => (x.Name, x.Title)).ToArray();
    }

    public void SelectActor(string actorName)
    {
        ClearButtonColor();
        _buttons[actorName].Background = Brushes.LightSkyBlue;
        UpdateContent(actorName);
    }

    private void UpdateContent(string actorName)
    {
        if (!_views.ContainsKey(actorName))
        {
            var view = _viewFactory.Create(typeof(EditableFrameView), _actorItemExplorerViewFactory.Create(actorName));
            _views[actorName] = view;
        }

        MyContentControl.Content = _views[actorName];
    }
}