using System.Collections.ObjectModel;

namespace ControlBeeWPF.ViewModels;

public class ActorItemTreeFilter
{
    public ActorItemTreeNode? Filter(ActorItemTreeNode root, string filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
            return CloneTree(root);

        return FilterInternal(root, filter.ToLowerInvariant()) ?? new ActorItemTreeNode(root.Data);
    }

    private ActorItemTreeNode? FilterInternal(ActorItemTreeNode node, string filter)
    {
        var selfMatch = IsMatch(node, filter);
        var matchedChildren = new ObservableCollection<ActorItemTreeNode>();

        foreach (var child in node.Children)
        {
            var filtered = FilterInternal(child, filter);
            if (filtered != null)
                matchedChildren.Add(filtered);
        }

        if (!selfMatch && matchedChildren.Count == 0)
            return null;

        var newNode = new ActorItemTreeNode(node.Data);
        foreach (var matchedChild in matchedChildren)
            newNode.Children.Add(matchedChild);

        return newNode;
    }

    private bool IsMatch(ActorItemTreeNode node, string filter)
    {
        return node.Data.Name.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
               node.Data.Title.Contains(filter, StringComparison.OrdinalIgnoreCase);
    }

    private ActorItemTreeNode CloneTree(ActorItemTreeNode node)
    {
        var newNode = new ActorItemTreeNode(node.Data);

        foreach (var child in node.Children)
            newNode.Children.Add(CloneTree(child));

        return newNode;
    }
}