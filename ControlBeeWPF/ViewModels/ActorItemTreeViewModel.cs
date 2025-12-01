using CommunityToolkit.Mvvm.ComponentModel;

namespace ControlBeeWPF.ViewModels;

public partial class ActorItemTreeViewModel : ObservableObject
{
    private ActorItemTree _actorItemTreeCollection = new(new ActorItemViewModel { Name = "root" });
    private ActorItemTree _filteredTree = new(new ActorItemViewModel { Name = "root" });

    private readonly ActorItemTreeFilter _filter = new();

    [ObservableProperty]
    private string _filterText = string.Empty;

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

    public ActorItemTree ActorItemTreeCollection
    {
        get => _actorItemTreeCollection;
        set
        {
            SetProperty(ref _actorItemTreeCollection, value);
            _filteredTree = _actorItemTreeCollection;
        }
    }
    
    public ActorItemTree FilteredTreeCollection
    {
        get => _filteredTree;
        set => SetProperty(ref _filteredTree, value);
    }
}
