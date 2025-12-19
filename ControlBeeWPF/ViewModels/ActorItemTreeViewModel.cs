using CommunityToolkit.Mvvm.ComponentModel;

namespace ControlBeeWPF.ViewModels;

public partial class ActorItemTreeViewModel : ObservableObject
{
    private readonly ActorItemTreeFilter _filter = new();
    private ActorItemTree _actorItemTreeCollection = new(new ActorItemViewModel { Name = "root" });
    private ActorItemTree _filteredTree = new(new ActorItemViewModel { Name = "root" });

    [ObservableProperty] private string _filterText = string.Empty;

    public ActorItemTree ActorItemTreeCollection
    {
        get => _actorItemTreeCollection;
        set
        {
            SetProperty(ref _actorItemTreeCollection, value);
            UpdateFilter();
        }
    }

    public ActorItemTree FilteredTreeCollection
    {
        get => _filteredTree;
        set => SetProperty(ref _filteredTree, value);
    }

    public void UpdateFilter()
    {
        var newRoot = _filter.Filter(ActorItemTreeCollection.Root, FilterText);
        if (newRoot != null)
            FilteredTreeCollection = new ActorItemTree(newRoot.Data) { Root = newRoot };
    }

    partial void OnFilterTextChanged(string value)
    {
        UpdateFilter();
    }
}