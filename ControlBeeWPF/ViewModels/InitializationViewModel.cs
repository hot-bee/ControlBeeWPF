using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControlBee.Constants;
using ControlBee.Interfaces;
using Message = ControlBee.Models.Message;

namespace ControlBeeWPF.ViewModels;

public partial class InitializationViewModel : ObservableObject, IDisposable
{
    private readonly IActorRegistry _actorRegistry;
    private readonly Dictionary<string, bool> _isInitializationChecked = new();
    private readonly IUiActor _ui;
    public Dictionary<string, InitializationStatus> InitializationStatus = new();
    private bool _initialized;
    private readonly HashSet<string> _excludedActors = [];

    public InitializationViewModel(IActorRegistry actorRegistry)
    {
        _actorRegistry = actorRegistry;
        _ui = (IUiActor)actorRegistry.Get("Ui")!;
        _ui.MessageArrived += UiOnMessageArrived;
    }

    public void EnsureInitialized(IEnumerable<string>? excludedActors)
    {
        if (_initialized)
            return;

        excludedActors ??= [];

        _excludedActors.Clear();
        foreach (var excludedActor in excludedActors)
            _excludedActors.Add(excludedActor);

        foreach (var (actorName, _) in GetActorTitles())
            SetInitialization(actorName, false);

        _initialized = true;
    }

    public bool InitializationAll { get; private set; }

    public void Dispose()
    {
        _ui.MessageArrived -= UiOnMessageArrived;
    }

    public (string actorName, string actorTitle)[] GetActorTitles() =>
        _actorRegistry
            .GetActorNames()
            .Where(actor => !_excludedActors.Contains(actor))
            .Select(actor => (actor, _actorRegistry.Get(actor)!.Title))
            .ToArray();

    private void UiOnMessageArrived(object? sender, Message e)
    {
        if (e.Name == "_status")
        {
            if (e.ActorName != "Syncer")
                return;
            var status = e.DictPayload!;

            var actorNames = _actorRegistry.GetActorNames();
            var receivedActorInitializationStatus =
                (Dictionary<string, object>)status["ActorInitializationStatus"]!;
            foreach (var actorName in actorNames)
            {
                if (!receivedActorInitializationStatus.TryGetValue(actorName, out var statusValue))
                    continue;
                var enumValue = Enum.Parse<InitializationStatus>(statusValue.ToString()!);
                InitializationStatus[actorName] = enumValue;
            }
            InitializationAll = (bool)status["AllInitialized"]!;
            OnPropertyChanged(nameof(InitializationStatus));
        }
    }

    [RelayCommand]
    private void Initialize()
    {
        var syncer = _actorRegistry.Get("Syncer")!;
        var initializingActors = new List<IActor>();
        foreach (var (actorName, check) in _isInitializationChecked)
        {
            if (!check)
                continue;
            var initializingActor = _actorRegistry.Get(actorName)!;
            initializingActors.Add(initializingActor);
        }
        syncer.Send(new Message(_ui, "_initialize", initializingActors));
    }

    public void SetInitialization(string actorName, bool check)
    {
        _isInitializationChecked[actorName] = check;
    }
}
