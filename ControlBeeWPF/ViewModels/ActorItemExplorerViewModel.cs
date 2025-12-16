using CommunityToolkit.Mvvm.ComponentModel;
using ControlBee.Interfaces;
using ControlBee.Models;
using ControlBee.Variables;
using Message = ControlBee.Models.Message;

namespace ControlBeeWPF.ViewModels;

public partial class ActorItemExplorerViewModel : ObservableObject, IDisposable
{
    private readonly IActor _actor;
    private readonly Dictionary<Guid, string> _dataIds = new();

    private readonly Dictionary<Guid, string> _metaIds = new();
    private readonly Dictionary<string, string> _names = new();
    private readonly IUiActor _uiActor;
    private readonly Dictionary<string, object> _values = new();
    private ActorItemTreeViewModel _actorItemTreeViewModel;

    [ObservableProperty] private ActorItemTreeNode? _selectedItem;

    public ActorItemExplorerViewModel(string actorName, IActorRegistry actorRegistry)
    {
        _actorItemTreeViewModel = new ActorItemTreeViewModel();
        _actor = actorRegistry.Get(actorName)!;

        _uiActor = (IUiActor)actorRegistry.Get("Ui")!;
        _uiActor.MessageArrived += UiActorOnMessageArrived;

        foreach (var (itemPath, type) in _actor.GetItems())
        {
            var metaId = _actor.Send(new ActorItemMessage(_uiActor, itemPath, "_itemMetaDataRead"));
            _metaIds[metaId] = itemPath;

            if (type.IsAssignableTo(typeof(IVariable)))
            {
                var dataId = _actor.Send(new ActorItemMessage(_uiActor, itemPath, "_itemDataRead"));
                _dataIds[dataId] = itemPath;
            }
        }
    }

    public ActorItemTreeViewModel ActorItemTreeViewModel
    {
        get => _actorItemTreeViewModel;
        set => SetProperty(ref _actorItemTreeViewModel, value);
    }

    public void Dispose()
    {
        _uiActor.MessageArrived -= UiActorOnMessageArrived;
    }

    private void UiActorOnMessageArrived(object? sender, Message e)
    {
        switch (e.Name)
        {
            case "_itemMetaData":
            {
                if (_metaIds.Remove(e.RequestId, out var itemPath))
                {
                    var name = e.DictPayload!["Name"] as string ?? string.Empty;
                    if (name.StartsWith('/'))
                        name = name.Split('/')[^1];
                    _names[itemPath] = name;

                    if (_metaIds.Count == 0 && _dataIds.Count == 0)
                        BuildTree();
                }
                else if (_metaIds.Count == 0 && _dataIds.Count == 0)
                {
                    if (e.DictPayload!.ContainsKey("Visible") || e.DictPayload!.ContainsKey("ItemPath"))
                    {
                        var name = e.DictPayload!["Name"] as string ?? string.Empty;
                        if (name.StartsWith('/'))
                            name = name.Split('/')[^1];

                        var visible = e.DictPayload!["Visible"] is true;
                        itemPath = (string?)e.DictPayload!["ItemPath"];
                        UpdateVisible(name, itemPath, visible);
                    }
                }

                break;
            }
            case "_itemDataChanged":
            {
                if (_dataIds.Remove(e.RequestId, out var itemPath))
                {
                    var valueChangedArgs = (ValueChangedArgs)
                        e.DictPayload![nameof(ValueChangedArgs)]!;
                    var newValue = valueChangedArgs.NewValue!;
                    _values[itemPath] = newValue;

                    if (_metaIds.Count == 0 && _dataIds.Count == 0)
                        BuildTree();
                }

                break;
            }
        }
    }

    private void UpdateVisible(string name, string? itemPath, bool visible)
    {
        if (string.IsNullOrEmpty(itemPath) || !_names.ContainsKey(itemPath))
            return;

        var sourceNode = _actorItemTreeViewModel.ActorItemTreeCollection.Root;
        var targetNode = _actorItemTreeViewModel.FilteredTreeCollection.Root;
        var pathNames = itemPath.Trim('/').Split("/");
        var targetName = string.Empty;
        for (var idx = 0; idx < pathNames.Length; idx++)
        {
            targetName = pathNames[idx];
            sourceNode = sourceNode.FindNode(targetName);
            if (sourceNode == null)
                return;

            if (targetNode?.FindNode(targetName) == null)
                break;

            if (idx < pathNames.Length - 1)
                targetNode = targetNode.FindNode(targetName);
        }

        sourceNode.Data.Visible = visible;

        if (visible)
        {
            var sourceIndex = sourceNode.Parent.Children.IndexOf(sourceNode);
            targetNode?.InsertChild(sourceIndex, sourceNode.Data);
        }
        else
        {
            targetNode?.RemoveChild(targetName);
        }
    }

    private void BuildTree()
    {
        foreach (var (itemPath, type) in _actor.GetItems())
        {
            var names = itemPath.Trim('/').Split("/");
            var node = _actorItemTreeViewModel.ActorItemTreeCollection.Root;
            var curItemPath = string.Empty;
            var variableItem = _actor.GetItem(itemPath) as IVariable;
            foreach (var name in names)
            {
                curItemPath += $"/{name}";
                var foundNode = node.FindNode(name);
                if (foundNode != null)
                {
                    node = foundNode;
                }
                else
                {
                    var newNode = node.AddChild(
                        new ActorItemViewModel { Name = name, ItemPath = curItemPath }
                    );
                    node = newNode;
                }
            }

            node.Data.Type = type;
            node.Data.Title = _names[itemPath];
            _values.TryGetValue(itemPath, out var value);
            node.Data.Value = value;
            node.Data.Scope = variableItem?.Scope;
        }

        _actorItemTreeViewModel.UpdateFilter();
    }
}