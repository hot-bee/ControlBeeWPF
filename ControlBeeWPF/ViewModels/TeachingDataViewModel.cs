using System.Data;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using ControlBee.Interfaces;
using ControlBee.Models;
using ControlBee.Variables;
using log4net;

namespace ControlBeeWPF.ViewModels;

public partial class TeachingDataViewModel : ObservableObject, IDisposable
{
    private static readonly ILog Logger = LogManager.GetLogger(
        MethodBase.GetCurrentMethod()!.DeclaringType!
    );

    private readonly List<ActorItemBinder> _axisItemBinderList = [];
    private readonly DataRow _currentPositionRow;

    private readonly DataRow _itemValueRow;

    private readonly ActorItemBinder _positionBinder;

    [ObservableProperty]
    private DataTable _tableData;

    public TeachingDataViewModel(string actorName, string itemPath, IActorRegistry actorRegistry)
    {
        _positionBinder = new ActorItemBinder(actorRegistry, actorName, itemPath);
        _positionBinder.MetaDataChanged += PositionBinderOnMetaDataChanged;
        _positionBinder.DataChanged += PositionBinderOnDataChanged;

        var actor = actorRegistry.Get(actorName)!;
        var axisItemPaths = actor.GetAxisItemPaths(itemPath);

        TableData = new DataTable { Columns = { "Item" } };

        foreach (var axisItemPath in axisItemPaths)
        {
            // DataTable columns shouldn't have any slashes.
            // See https://stackoverflow.com/questions/50327749/c-sharp-datatable-datagrid-special-character-slash-in-column-names.
            TableData.Columns.Add(axisItemPath.Trim('/'), typeof(double));
            var axisItemBinder = new ActorItemBinder(actorRegistry, actorName, axisItemPath);
            _axisItemBinderList.Add(axisItemBinder);
            axisItemBinder.MetaDataChanged += AxisItemBinderOnMetaDataChanged;
            axisItemBinder.DataChanged += AxisItemBinderOnDataChanged;
        }

        _currentPositionRow = TableData.Rows.Add("Current");
        _itemValueRow = TableData.Rows.Add("Name");
    }

    private void AxisItemBinderOnMetaDataChanged(object? sender, Dictionary<string, object?> e)
    {
        var index = _axisItemBinderList.IndexOf((ActorItemBinder)sender!);
        var name = (string)e["Name"]!;
        TableData.Columns[index + 1].ColumnName = name.Trim('/');
    }

    public void Dispose()
    {
        _positionBinder.MetaDataChanged -= PositionBinderOnMetaDataChanged;
        _positionBinder.DataChanged -= PositionBinderOnDataChanged;
        _positionBinder.Dispose();
        _axisItemBinderList.ForEach(x =>
        {
            x.MetaDataChanged -= AxisItemBinderOnMetaDataChanged;
            x.DataChanged -= AxisItemBinderOnDataChanged;
            x.Dispose();
        });
    }

    private void AxisItemBinderOnDataChanged(object? sender, Dictionary<string, object?> e)
    {
        var index = _axisItemBinderList.IndexOf((ActorItemBinder)sender!);
        var newValue = e["CommandPosition"];
        _currentPositionRow[index + 1] = newValue;
    }

    private void PositionBinderOnMetaDataChanged(object? sender, Dictionary<string, object?> e)
    {
        _itemValueRow[0] = (string)e["Name"]!;
    }

    private void PositionBinderOnDataChanged(object? sender, Dictionary<string, object?> e)
    {
        var newValue = e.GetValueOrDefault("NewValue")!;
        if (e.GetValueOrDefault("NewValue") is Position1D position1D)
        {
            _itemValueRow[1] = position1D.Values[0];
        }
        else if (e.GetValueOrDefault("NewValue") is Position2D position2D)
        {
            _itemValueRow[1] = position2D.Values[0];
            _itemValueRow[2] = position2D.Values[1];
        }
        else if (e.GetValueOrDefault("NewValue") is Position3D position3D)
        {
            _itemValueRow[1] = position3D.Values[0];
            _itemValueRow[2] = position3D.Values[1];
            _itemValueRow[3] = position3D.Values[2];
        }
        else if (e.GetValueOrDefault("NewValue") is Position4D position4D)
        {
            _itemValueRow[1] = position4D.Values[0];
            _itemValueRow[2] = position4D.Values[1];
            _itemValueRow[3] = position4D.Values[2];
            _itemValueRow[4] = position4D.Values[3];
        }
        else
        {
            Logger.Error($"Unknown item type. ({newValue.GetType()})");
        }
    }
}
