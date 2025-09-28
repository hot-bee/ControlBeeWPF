using System.Windows.Threading;
using ControlBee.Interfaces;
using ControlBee.Models;
using Message = ControlBee.Models.Message;

namespace ControlBeeWPF.Models;

public class UiActorMessageHandler(Dispatcher dispatcher) : IUiActorMessageHandler
{
    private Action<Message>? _publishMessage;

    public void ProcessMessage(Message message)
    {
        dispatcher.InvokeAsync(() =>
        {
            Publish(message);
        });
    }

    public void SetCallback(Action<Message> publishMessage)
    {
        _publishMessage = publishMessage;
    }

    private void Publish(Message innerMessage)
    {
        _publishMessage?.Invoke(innerMessage);
    }
}
