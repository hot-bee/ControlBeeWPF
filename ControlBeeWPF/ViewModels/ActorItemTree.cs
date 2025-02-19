namespace ControlBeeWPF.ViewModels;

public class ActorItemTree(ActorItemViewModel data)
{
    public ActorItemTreeNode Root { get; set; } = new(data);
}
