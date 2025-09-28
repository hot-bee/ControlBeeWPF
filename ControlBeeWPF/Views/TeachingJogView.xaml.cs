using System.Windows.Controls;
using ControlBeeWPF.ViewModels;
using log4net;
using UserControl = System.Windows.Controls.UserControl;

namespace ControlBeeWPF.Views;

/// <summary>
///     Interaction logic for TeachingJogView.xaml
/// </summary>
public partial class TeachingJogView : UserControl, IDisposable
{
    private static readonly ILog Logger = LogManager.GetLogger("Ui");

    public TeachingJogView(TeachingJogViewModel viewModel, UserControl jogView)
    {
        DataContext = viewModel;
        InitializeComponent();
        JogControlArea.Content = jogView;
    }

    public void Dispose()
    {
        // Empty
    }
}