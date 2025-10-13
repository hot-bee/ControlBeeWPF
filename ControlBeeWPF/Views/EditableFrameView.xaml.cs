using System.Windows;
using ControlBee.Interfaces;
using ControlBeeWPF.ViewModels;

namespace ControlBeeWPF.Views;

/// <summary>
///     Interaction logic for EditableFrame.xaml
/// </summary>
public partial class EditableFrameView: IDisposable
{
    public EditableFrameView(ISystemConfigurations systemConfigurations, EditableFrameViewModel viewModel, System.Windows.Controls.UserControl content)
    {
        DataContext = viewModel;
        InitializeComponent();
        if (systemConfigurations.AutoVariableSave) ControlPanel.Visibility = Visibility.Collapsed;
        MyContent.Content = content;
    }

    public void Dispose()
    {
        if(MyContent.Content is IDisposable disposable) disposable.Dispose();
    }
}