namespace ControlBeeWPF.Interfaces;

public interface IDialogService
{
    public bool Confirm(string message, string title = "Confirm");
}
