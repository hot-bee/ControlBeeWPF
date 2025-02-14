using CommunityToolkit.Mvvm.ComponentModel;
using ControlBee.Interfaces;

namespace ControlBeeWPF.ViewModels;

public class AxesStatusViewModel(IActorRegistry actorRegistry) : ObservableObject
{
    public IActor[] SelectedActors { get; private set; } = [];

    private IActor[] GetActors()
    {
        return actorRegistry.GetActors().Where(x => GetAxisItemPaths(x).Length > 0).ToArray();
    }

    public (string name, string Title)[] GetActorNameTitlePairs()
    {
        return GetActors().Select(x => (x.Name, x.Title)).ToArray();
    }

    private string[] GetAxisItemPaths(IActor actor)
    {
        var list = new List<string>();
        foreach (var (itemPath, type) in actor.GetItems())
            if (type.IsAssignableTo(typeof(IAxis)))
                list.Add(itemPath);

        return list.ToArray();
    }

    public void SelectActor(string name)
    {
        var newList = new List<IActor>();
        if (string.IsNullOrEmpty(name))
            newList.AddRange(GetActors());
        else
            newList.Add(actorRegistry.Get(name)!);

        SelectedActors = newList.ToArray();
        OnPropertyChanged(nameof(SelectedActors));
    }
}
