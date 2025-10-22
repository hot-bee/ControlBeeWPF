using ControlBee.Interfaces;
using ControlBeeWPF.Interfaces;
using System.Windows;
using System.Windows.Controls;

namespace ControlBeeWPF.Views;

/// <summary>
/// Interaction logic for IoListView.xaml
/// </summary>
public partial class IoListView
{
    private readonly IActor _actor;
    private readonly string _actorName;
    private readonly int _columns;
    private readonly IViewFactory _viewFactory;

    public IoListView(string actorName, int columns, IActorRegistry actorRegistry, IViewFactory viewFactory)
    {
        _actorName = actorName;
        _columns = columns;
        _viewFactory = viewFactory;
        _actor = actorRegistry.Get(actorName)!;
        InitializeComponent();

        SetupGrid(InputGrid, typeof(IDigitalInput), typeof(DigitalInputStatusBarView));
        SetupGrid(OutputGrid, typeof(IDigitalOutput), typeof(DigitalOutputStatusBarView));
    }

    private void SetupGrid(Grid grid, Type ioType, Type viewType)
    {
        var items = _actor.GetItems();
        var itemPaths = new List<string>();
        foreach (var (itemPath, type) in items)
            if (type.IsAssignableTo(ioType))
                itemPaths.Add(itemPath);

        for (var i = 0; i < _columns; i++) grid.ColumnDefinitions.Add(new ColumnDefinition());

        var numberOfRows = (int)Math.Ceiling((double)itemPaths.Count / _columns);
        for (var i = 0; i < numberOfRows; i++)
        {
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(0, GridUnitType.Auto) });
            for (var j = 0; j < _columns; j++)
            {
                var index = i * _columns + j;
                if (itemPaths.Count <= index) continue;

                var itemPath = itemPaths[index];
                var view = _viewFactory.Create(viewType, _actorName, itemPath);
                Grid.SetRow(view, i);
                Grid.SetColumn(view, j);
                grid.Children.Add(view);
            }
        }
    }
}
