using CommunityToolkit.Mvvm.ComponentModel;

namespace ControlBeeWPF.ViewModels;

public class ActorItemTreeViewModel : ObservableObject
{
    private ActorItemTree _actorItemTreeCollection = new(new ActorItemViewModel { Name = "root" });

    public ActorItemTree ActorItemTreeCollection
    {
        get => _actorItemTreeCollection;
        set => SetProperty(ref _actorItemTreeCollection, value);
    }
}
