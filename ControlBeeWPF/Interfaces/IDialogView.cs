namespace ControlBeeWPF.Interfaces;

public interface IDialogView
{
    void ShowView();
    event EventHandler? DialogClosed;
}
