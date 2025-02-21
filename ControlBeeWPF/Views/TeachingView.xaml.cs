using System.Reflection;
using System.Windows.Controls;
using ControlBeeWPF.Services;
using ControlBeeWPF.ViewModels;
using log4net;

namespace ControlBeeWPF.Views;

/// <summary>
///     Interaction logic for TeachingView.xaml
/// </summary>
public partial class TeachingView : UserControl, IDisposable
{
    private static readonly ILog Logger = LogManager.GetLogger(
        MethodBase.GetCurrentMethod()!.DeclaringType!
    );

    private readonly string _actorName;

    private readonly Dictionary<string, TeachingDataView> _dataViews = [];
    private readonly TeachingViewFactory _teachingViewFactory;
    private readonly TeachingJogViewFactory _teachingJogViewFactory;
    private readonly TeachingViewModel _viewModel;

    public TeachingView(
        string actorName,
        TeachingViewModel viewModel,
        TeachingViewFactory teachingViewFactory,
        TeachingAxisStatusViewFactory teachingAxisStatusViewFactory,
        TeachingJogViewFactory teachingJogViewFactory
    )
    {
        _actorName = actorName;
        _teachingViewFactory = teachingViewFactory;
        _teachingJogViewFactory = teachingJogViewFactory;
        InitializeComponent();
        DataContext = viewModel;
        _viewModel = viewModel;
        _viewModel.Loaded += ViewModelOnLoaded;
        AxisStatusContent.Content = teachingAxisStatusViewFactory.Create(_actorName);
    }

    public void Dispose()
    {
        _viewModel.Loaded -= ViewModelOnLoaded;
        foreach (var view in _dataViews.Values)
            view.Dispose();
        if (AxisStatusContent.Content is IDisposable disposable)
            disposable.Dispose();
    }

    private void ViewModelOnLoaded(object? sender, EventArgs e)
    {
        foreach (var itemPath in _viewModel.PositionItemPaths)
        {
            var name = _viewModel.ItemNames[itemPath];
            PositionItemList.Items.Add(name);
        }
    }

    private void PositionItemList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            var itemPath = _viewModel.PositionItemPaths[PositionItemList.SelectedIndex];
            if (!_dataViews.TryGetValue(itemPath, out var view))
            {
                view = _teachingViewFactory.CreateData(_actorName, itemPath);
                _dataViews[itemPath] = view;
            }

            DataContent.Content = view;
            JogContent.Content = _teachingJogViewFactory.Create(_actorName, itemPath); // TODO: memory leak
        }
        catch (ArgumentOutOfRangeException exception)
        {
            Logger.Error("Selection index is out of range.", exception);
        }
    }
}
