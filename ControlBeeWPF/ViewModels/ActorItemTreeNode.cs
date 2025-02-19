using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ControlBeeWPF.ViewModels;

public class ActorItemTreeNode(ActorItemViewModel data) : ObservableObject
{
    private ObservableCollection<ActorItemTreeNode> _children = [];

    public ActorItemViewModel Data { get; set; } = data;

    public ObservableCollection<ActorItemTreeNode> Children
    {
        get => _children;
        set => SetProperty(ref _children, value);
    }

    [AllowNull]
    public ActorItemTreeNode Parent { get; set; }

    public ActorItemTreeNode AddChild(ActorItemViewModel data)
    {
        var child = new ActorItemTreeNode(data) { Parent = this };
        _children.Add(child);
        return child;
    }

    public void RemoveChild(ActorItemViewModel data)
    {
        throw new NotImplementedException();
    }

    public ActorItemTreeNode? FindNode(string name)
    {
        return Children.FirstOrDefault(child => child.Data.Name == name);
    }

    public ActorItemTreeNode? FindNode(ActorItemViewModel data)
    {
        if (EqualityComparer<ActorItemViewModel>.Default.Equals(Data, data))
            return this;

        foreach (var child in Children)
        {
            var foundNode = child.FindNode(data);
            if (foundNode != null)
                return foundNode;
        }

        return null;
    }
}
