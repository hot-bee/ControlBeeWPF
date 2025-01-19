using System.Windows.Threading;
using ControlBee.Interfaces;
using ControlBee.Models;

namespace ControlBeeWPF.Models;

public class UiActorMessageHandler(Dispatcher dispatcher) : IUiActorMessageHandler
{
    private Action<Message>? _publishMessage;

    public void ProcessMessage(Message message)
    {
        dispatcher.Invoke(
            (Message innerMessage) =>
            {
                _publishMessage?.Invoke(innerMessage);
            },
            message
        );
    }

    public void SetCallback(Action<Message> publishMessage)
    {
        _publishMessage = publishMessage;
    }
}
