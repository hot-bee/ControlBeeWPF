using System.Windows;
using ControlBeeWPF.Interfaces;
using MessageBox = System.Windows.MessageBox;

namespace ControlBeeWPF.Services;

public class DialogService : IDialogService
{
    public bool Confirm(string message, string title = "Confirm")
    {
        return MessageBox.Show(
            message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
    }
}