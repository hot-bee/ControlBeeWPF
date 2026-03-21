using System.Windows.Controls;
using ControlBee.Services;
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

        TranslateHeader(MoveToPositionGroup, "TeachingJogView.MoveToPosition");
        TranslateContent(MoveToHomeButton, "TeachingJogView.MoveToHomePos");
        TranslateContent(MoveToSavedPosButton, "TeachingJogView.MoveToSavedPos");
        TranslateHeader(JogControlGroup, "TeachingJogView.JogControl");
        TranslateHeader(PositionSavingGroup, "TeachingJogView.PositionSaving");
        TranslateContent(SetPosButton, "TeachingJogView.SetPos");
        TranslateContent(RestoreButton, "TeachingJogView.Restore");
    }

    private static void TranslateHeader(HeaderedContentControl control, string key)
    {
        var text = LocalizationManager.Instance.Translate(key);
        if (!string.IsNullOrEmpty(text))
            control.Header = text;
    }

    private static void TranslateContent(ContentControl control, string key)
    {
        var text = LocalizationManager.Instance.Translate(key);
        if (!string.IsNullOrEmpty(text))
            control.Content = text;
    }

    public void Dispose()
    {
        // Empty
    }
}
