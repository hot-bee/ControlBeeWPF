using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ControlBeeWPF.ViewModels;

public partial class ActorItemTreeViewModel : ObservableObject
{
    private ActorItemTree _actorItemTreeCollection = new(new ActorItemViewModel { Name = "root" });
    private string _filterText = string.Empty;

    public ActorItemTree ActorItemTreeCollection
    {
        get => _actorItemTreeCollection;
        set
        {
            SetProperty(ref _actorItemTreeCollection, value);
            ApplyFilter();
        }
    }

    public string FilterText
    {
        get => _filterText;
        set
        {
            if (SetProperty(ref _filterText, value))
                ApplyFilter();
        }
    }

    public void ApplyFilter()
    {
        foreach (var child in ActorItemTreeCollection.Root.Children)
            child.ApplyFilter(FilterText);
    }

}