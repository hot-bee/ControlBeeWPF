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
    private readonly object[] _location;

    private static readonly ILog Logger = LogManager.GetLogger(
        MethodBase.GetCurrentMethod()!.DeclaringType!
    );

    private readonly List<ActorItemBinder> _axisItemBinderList = [];
    private readonly List<double> _displayResolutions = [];
    private readonly List<double?> _rawCurrentPositions = [];
    private readonly List<double?> _rawItemValues = [];
    private readonly DataRow _currentPositionRow;

    private readonly DataRow _itemValueRow;

    private readonly ActorItemBinder _positionBinder;

    [ObservableProperty]
    private DataTable _tableData;

    public TeachingDataViewModel(
        string actorName,
        string itemPath,
        object[] location,
        IActorRegistry actorRegistry
    )
    {
        _location = location;
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
            _displayResolutions.Add(1.0);
            _rawCurrentPositions.Add(null);
            _rawItemValues.Add(null);
            axisItemBinder.MetaDataChanged += AxisItemBinderOnMetaDataChanged;
            axisItemBinder.DataChanged += AxisItemBinderOnDataChanged;
        }

        _currentPositionRow = TableData.Rows.Add("Current");
        _itemValueRow = TableData.Rows.Add("Name");
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

    private void AxisItemBinderOnMetaDataChanged(object? sender, Dictionary<string, object?> e)
    {
        var index = _axisItemBinderList.IndexOf((ActorItemBinder)sender!);
        var name = (string)e["Name"]!;
        TableData.Columns[index + 1].ColumnName = name.Trim('/');
        _displayResolutions[index] = e.GetValueOrDefault("DisplayResolution") as double? ?? 1.0;
        if (_rawCurrentPositions[index] is { } rawCurrentPosition)
            _currentPositionRow[index + 1] = rawCurrentPosition * _displayResolutions[index];
        if (_rawItemValues[index] is { } rawItemValue)
            _itemValueRow[index + 1] = rawItemValue * _displayResolutions[index];
    }

    private void AxisItemBinderOnDataChanged(object? sender, Dictionary<string, object?> e)
    {
        var index = _axisItemBinderList.IndexOf((ActorItemBinder)sender!);
        var newValue = (double)e["CommandPosition"]!;
        _rawCurrentPositions[index] = newValue;
        _currentPositionRow[index + 1] = newValue * _displayResolutions[index];
    }

    private void SetItemValue(int axisIndex, object? value)
    {
        if (axisIndex >= _rawItemValues.Count || value is not double rawValue)
        {
            _itemValueRow[1 + axisIndex] = value;
            return;
        }

        _rawItemValues[axisIndex] = rawValue;
        _itemValueRow[1 + axisIndex] = rawValue * _displayResolutions[axisIndex];
    }

    private void PositionBinderOnMetaDataChanged(object? sender, Dictionary<string, object?> e)
    {
        _itemValueRow[0] = (string)e["Name"]!;
    }

    private void PositionBinderOnDataChanged(object? sender, Dictionary<string, object?> e)
    {
        var valueChangedArgs = e[nameof(ValueChangedArgs)] as ValueChangedArgs;
        var newValue = valueChangedArgs?.NewValue;
        var location = valueChangedArgs?.Location;

        foreach (var key in _location)
        {
            if (location?.Length > 0)
            {
                if (!location[0].Equals(key))
                    return;
                location = location[1..];
                continue;
            }
            if (newValue is IIndex1D index1D)
            {
                newValue = index1D.GetValue((int)key);
            }
            else
                throw new ArgumentException();
        }

        if (location?.Length > 0)
        {
            var index = (int)location[0]!;
            SetItemValue(index, newValue);
        }
        else
        {
            if (newValue is Position1D position1D)
            {
                SetItemValue(0, position1D.Values[0]);
            }
            else if (newValue is Position2D position2D)
            {
                SetItemValue(0, position2D.Values[0]);
                SetItemValue(1, position2D.Values[1]);
            }
            else if (newValue is Position3D position3D)
            {
                SetItemValue(0, position3D.Values[0]);
                SetItemValue(1, position3D.Values[1]);
                SetItemValue(2, position3D.Values[2]);
            }
            else if (newValue is Position4D position4D)
            {
                SetItemValue(0, position4D.Values[0]);
                SetItemValue(1, position4D.Values[1]);
                SetItemValue(2, position4D.Values[2]);
                SetItemValue(3, position4D.Values[3]);
            }
            else
            {
                Logger.Error($"Unknown item type. ({newValue?.GetType()})");
            }
        }
    }
}
