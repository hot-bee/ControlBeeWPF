using System.Data;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using ControlBee.Interfaces;

namespace ControlBeeWPF.ViewModels;

public partial class ActorMonitorViewModel : ObservableObject, IDisposable
{
    private readonly IUiActor _uiActor;
    private readonly Dictionary<IActor, DataRow> _rows = new();

    [ObservableProperty]
    private DataTable _data = new();

    public ActorMonitorViewModel(IActorRegistry actorRegistry)
    {
        _uiActor = (IUiActor)actorRegistry.Get("ui")!;
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
        }
    }

    private void UiActor_MessageArrived(object? sender, ControlBee.Models.Message e)
    {
        switch (e.Name)
        {
            case "_stateChanged":
            {
                var stateName = (string)e.Payload!;
                _rows[e.Sender][1] = stateName;
                break;
            }
            case "_status":
            {
                var jsonString = JsonSerializer.Serialize(
                    e.DictPayload,
                    new JsonSerializerOptions() { WriteIndented = true }
                );
                _rows[e.Sender][2] = jsonString;
                break;
            }
        }
    }

    public void Dispose()
    {
        _uiActor.MessageArrived -= UiActor_MessageArrived;
    }
}
