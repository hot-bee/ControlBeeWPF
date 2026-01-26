using ControlBee.Interfaces;
using ControlBeeWPF.Interfaces;
using Message = ControlBee.Models.Message;

namespace ControlBeeWPF.Services;

public class DialogDisplay
{
    private readonly HashSet<IDialogContext> _onContexts = [];
    private readonly IViewFactory _viewFactory;

    public DialogDisplay(IActorRegistry actorRegistry, IViewFactory viewFactory)
    {
        _viewFactory = viewFactory;
        var ui = (IUiActor)actorRegistry.Get("Ui")!;
        ui.MessageArrived += Ui_MessageArrived;
    }

    private void Ui_MessageArrived(object? sender, Message e)
    {
        switch (e.Name)
        {
            case "_displayDialog":
            {
                var context = (IDialogContext)e.Payload!;
                if (_onContexts.Contains(context))
                    return;
                var dialog = _viewFactory.Create<IDialogView>()!;
                dialog.Show(context, e);
                _onContexts.Add(context);
                dialog.DialogClosed += (o, args) =>
                {
                    _onContexts.Remove(context);
                };
                break;
            }
            case "_closeDialog":
            {
                var context = (IDialogContext)e.Payload!;
                if (!_onContexts.Contains(context))
                    return;
                context.Close();
                break;
            }
        }
    }
}
