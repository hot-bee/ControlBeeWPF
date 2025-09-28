using System.Data;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using ControlBee.Interfaces;
using ControlBee.Models;
using Dict = System.Collections.Generic.Dictionary<string, object?>;
using Message = ControlBee.Models.Message;

namespace ControlBeeWPF.ViewModels;

public partial class ActorMonitorViewModel : ObservableObject, IDisposable
{
    private readonly IActorRegistry _actorRegistry;
    private readonly Dictionary<IActor, DataRow> _rows = new();
    private readonly IUiActor _uiActor;

    [ObservableProperty] private Dictionary<string, string> _actorStatus = [];

    [ObservableProperty] private DataTable _data = new();

    public ActorMonitorViewModel(IActorRegistry actorRegistry)
    {
        _actorRegistry = actorRegistry;
        _uiActor = (IUiActor)actorRegistry.Get("Ui")!;
        _uiActor.MessageArrived += UiActor_MessageArrived;

        _data.Columns.Add("Actor", typeof(string));
        _data.Columns.Add("State", typeof(string));
        _data.Columns.Add("Status", typeof(string));
        foreach (var actorName in actorRegistry.GetActorNames())
        {
            var actor = actorRegistry.Get(actorName)!;
            var row = _data.NewRow();
            row[0] = actor.Name;
            _data.Rows.Add(row);
            _rows[actor] = row;
            ActorStatus[actor.Name] = "";
        }
    }

    public void Dispose()
    {
        _uiActor.MessageArrived -= UiActor_MessageArrived;
    }

    private void UiActor_MessageArrived(object? sender, Message e)
    {
        switch (e.Name)
        {
            case "_stateChanged":
            {
                var stateName = (string)e.Payload!;
                _rows[e.Sender][1] = stateName;
                ActorStatus[e.Sender.Name] = stateName;
                OnPropertyChanged(nameof(ActorStatus));
                break;
            }
            case "_status":
            {
                var jsonString = JsonSerializer.Serialize(
                    e.DictPayload,
                    new JsonSerializerOptions { WriteIndented = true }
                );
                _rows[e.Sender][2] = jsonString;
                break;
            }
        }
    }

    public void Poke(int rowIndex)
    {
        var actorName = (string)_data.Rows[rowIndex][0];
        var actor = _actorRegistry.Get(actorName)!;
        actor.Send(new Message(_uiActor, "_status", new Dict()));
    }
}