using CommunityToolkit.Mvvm.ComponentModel;
using ControlBee.Interfaces;
using ControlBee.Models;
using ControlBee.Variables;

namespace ControlBeeWPF.ViewModels;

public class TeachingViewModel : ObservableObject, IDisposable
{
    private readonly Dictionary<Guid, string> _dataIds = new();
    private readonly Dictionary<Guid, string> _metaIds = new();
    private readonly IUiActor _uiActor;
    public Dictionary<string, string> ItemNames = [];
    public Dictionary<string, Type> ItemTypes = [];

    public TeachingViewModel(string actorName, IActorRegistry actorRegistry)
    {
        _uiActor = (IUiActor)actorRegistry.Get("Ui")!;
        var actor = actorRegistry.Get(actorName)!;

        _uiActor.MessageArrived += UiActorOnMessageArrived;
        foreach (var (itemPath, type) in actor.GetItems())
            if (type.IsAssignableTo(typeof(IVariable)))
            {
                var metaId = actor.Send(
                    new ActorItemMessage(_uiActor, itemPath, "_itemMetaDataRead")
                );
                _metaIds[metaId] = itemPath;

                var dataId = actor.Send(new ActorItemMessage(_uiActor, itemPath, "_itemDataRead"));
                _dataIds[dataId] = itemPath;
            }
    }

    public List<string> PositionItemPaths { get; } = [];

    public void Dispose()
    {
        _uiActor.MessageArrived -= UiActorOnMessageArrived;
    }

    public event EventHandler? Loaded;

    private void UiActorOnMessageArrived(object? sender, Message e)
    {
        switch (e.Name)
        {
            case "_itemMetaData":
                if (_metaIds.ContainsKey(e.RequestId))
                {
                    var actorItemMessage = (ActorItemMessage)e;
                    ItemNames[actorItemMessage.ItemPath] =
                        e.DictPayload!["Name"] as string ?? string.Empty;
                    if (ItemTypes.Count == _dataIds.Count && ItemNames.Count == _dataIds.Count)
                        OnLoaded();
                }

                break;
            case "_itemDataChanged":
                if (_dataIds.ContainsKey(e.RequestId))
                {
                    var actorItemMessage = (ActorItemMessage)e;
                    var valueChangedArgs = (ValueChangedArgs)
                        e.DictPayload![nameof(ValueChangedArgs)]!;
                    var type = valueChangedArgs.NewValue!.GetType();
                    ItemTypes[actorItemMessage.ItemPath] = type;
                    if (type.IsAssignableTo(typeof(Position)))
                        PositionItemPaths.Add(actorItemMessage.ItemPath);
                    if (ItemTypes.Count == _dataIds.Count && ItemNames.Count == _dataIds.Count)
                        OnLoaded();
                }

                break;
        }
    }

    protected virtual void OnLoaded()
    {
        Loaded?.Invoke(this, EventArgs.Empty);
    }
}
