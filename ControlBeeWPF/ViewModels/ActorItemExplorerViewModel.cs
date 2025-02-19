using CommunityToolkit.Mvvm.ComponentModel;
using ControlBee.Interfaces;
using ControlBee.Models;

namespace ControlBeeWPF.ViewModels;

public partial class ActorItemExplorerViewModel : ObservableObject, IDisposable
{
    private readonly IActor _actor;
    private readonly Dictionary<string, string> _names = new();

    private readonly Dictionary<Guid, string> _sentIds = new();
    private readonly IUiActor _uiActor;
    private ActorItemTreeViewModel _actorItemTreeViewModel;

    [ObservableProperty]
    private ActorItemTreeNode? _selectedItem;

    public ActorItemExplorerViewModel(string actorName, IActorRegistry actorRegistry)
    {
        _actorItemTreeViewModel = new ActorItemTreeViewModel();
        _actor = actorRegistry.Get(actorName)!;

        _uiActor = (IUiActor)actorRegistry.Get("ui")!;
        _uiActor.MessageArrived += UiActorOnMessageArrived;

        foreach (var (itemPath, type) in _actor.GetItems())
        {
            var id = _actor.Send(new ActorItemMessage(_uiActor, itemPath, "_itemMetaDataRead"));
            _sentIds[id] = itemPath;
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
        if (e.Name != "_itemMetaData" || !_sentIds.TryGetValue(e.RequestId, out var itemPath))
            return;
        var name = e.DictPayload!["Name"] as string ?? string.Empty;
        if (name.StartsWith('/'))
            name = name.Split('/')[^1];
        _names[itemPath] = name;

        if (_names.Count == _sentIds.Count)
            BuildTree();
    }

    private void BuildTree()
    {
        foreach (var (itemPath, type) in _actor.GetItems())
        {
            var names = itemPath.Trim('/').Split("/");
            var node = _actorItemTreeViewModel.ActorItemTreeCollection.Root;
            var curItemPath = string.Empty;
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
        }
    }
}
