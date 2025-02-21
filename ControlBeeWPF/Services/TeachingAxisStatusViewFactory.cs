using ControlBee.Interfaces;
using ControlBeeWPF.Views;

namespace ControlBeeWPF.Services;

public class TeachingAxisStatusViewFactory(
    IActorRegistry actorRegistry,
    AxisStatusViewFactory axisStatusViewFactory
)
{
    public TeachingAxisStatusView Create(string actorName)
    {
        return new TeachingAxisStatusView(actorName, actorRegistry, axisStatusViewFactory);
    }
}
