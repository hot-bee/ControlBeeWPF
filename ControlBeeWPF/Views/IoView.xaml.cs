using System.Windows;
using System.Windows.Controls;
using ControlBee.Interfaces;
using ControlBeeWPF.Interfaces;
using Button = System.Windows.Controls.Button;
using HorizontalAlignment = System.Windows.HorizontalAlignment;

namespace ControlBeeWPF.Views;

/// <summary>
///     Interaction logic for IoView.xaml
/// </summary>
public partial class IoView
{
    private readonly int _pageSize;

    private readonly IViewFactory _viewFactory;
    private readonly string _actorName;
    private readonly int _columns;

    private readonly List<string> _inputPaths = [];
    private readonly List<string> _outputPaths = [];

    private int _inputPageIndex;
    private int _outputPageIndex;

    public IoView(
        string actorName,
        int columns,
        IActorRegistry actorRegistry,
        IViewFactory viewFactory,
        int? pageSize = 32
    )
    {
        _actorName = actorName;
        _columns = columns;
        _pageSize = pageSize ?? 32;
        _viewFactory = viewFactory;
        var actor = actorRegistry.Get(actorName)!;

        InitializeComponent();

        foreach (var (itemPath, type) in actor.GetItems())
        {
            if (type.IsAssignableTo(typeof(IDigitalInput)))
                _inputPaths.Add(itemPath);

            if (type.IsAssignableTo(typeof(IDigitalOutput)))
                _outputPaths.Add(itemPath);
        }

        InputFirstButton.Click += (_, __) => GoToInputPage(0);
        InputPrevButton.Click += (_, __) => GoToInputPage(_inputPageIndex - 1);
        InputNextButton.Click += (_, __) => GoToInputPage(_inputPageIndex + 1);
        InputLastButton.Click += (_, __) => GoToInputPage(GetTotalPages(_inputPaths.Count) - 1);

        OutputFirstButton.Click += (_, __) => GoToOutputPage(0);
        OutputPrevButton.Click += (_, __) => GoToOutputPage(_outputPageIndex - 1);
        OutputNextButton.Click += (_, __) => GoToOutputPage(_outputPageIndex + 1);
        OutputLastButton.Click += (_, __) => GoToOutputPage(GetTotalPages(_outputPaths.Count) - 1);

        GoToInputPage(0);
        GoToOutputPage(0);
    }

    private int GetTotalPages(int totalCount) =>
        Math.Max(1, (int)Math.Ceiling(totalCount / (double)_pageSize));

    private void GoToInputPage(int pageIndex)
    {
        var total = GetTotalPages(_inputPaths.Count);
        _inputPageIndex = ClampPage(pageIndex, total);

        RenderPage(InputGrid, _inputPaths, _inputPageIndex, typeof(DigitalInputStatusBarView));
        RefreshInputPagerUi();
    }

    private void GoToOutputPage(int pageIndex)
    {
        var total = GetTotalPages(_outputPaths.Count);
        _outputPageIndex = ClampPage(pageIndex, total);

        RenderPage(OutputGrid, _outputPaths, _outputPageIndex, typeof(DigitalOutputStatusBarView));
        RefreshOutputPagerUi();
    }

    private static int ClampPage(int pageIndex, int totalPages)
    {
        if (totalPages <= 0)
            return 0;
        if (pageIndex < 0)
            return 0;
        if (pageIndex > totalPages - 1)
            return totalPages - 1;
        return pageIndex;
    }

    private void RefreshInputPagerUi()
    {
        var total = GetTotalPages(_inputPaths.Count);

        InputFirstButton.IsEnabled = _inputPageIndex > 0;
        InputPrevButton.IsEnabled = _inputPageIndex > 0;
        InputNextButton.IsEnabled = _inputPageIndex < total - 1;
        InputLastButton.IsEnabled = _inputPageIndex < total - 1;

        BuildPagerSimple(InputPager, total, _inputPageIndex, GoToInputPage);
    }

    private void RefreshOutputPagerUi()
    {
        var total = GetTotalPages(_outputPaths.Count);

        OutputFirstButton.IsEnabled = _outputPageIndex > 0;
        OutputPrevButton.IsEnabled = _outputPageIndex > 0;
        OutputNextButton.IsEnabled = _outputPageIndex < total - 1;
        OutputLastButton.IsEnabled = _outputPageIndex < total - 1;

        BuildPagerSimple(OutputPager, total, _outputPageIndex, GoToOutputPage);
    }

    private void BuildPagerSimple(
        WrapPanel pagerHost,
        int totalPages,
        int currentPageIndex,
        Action<int> onPageClicked
    )
    {
        pagerHost.Children.Clear();
        totalPages = Math.Max(1, totalPages);

        for (var pageIndex = 0; pageIndex < totalPages; pageIndex++)
        {
            var isActive = pageIndex == currentPageIndex;

            var style =
                (Style?)TryFindResource(isActive ? "PagerButtonActiveStyle" : "PagerButtonStyle")
                ?? (Style?)TryFindResource("PagerButtonStyle");

            var button = new Button
            {
                Content = (pageIndex + 1).ToString(),
                Style = style,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            int index = pageIndex;
            button.Click += (_, __) => onPageClicked(index);

            pagerHost.Children.Add(button);
        }
    }

    private void RenderPage(Grid grid, List<string> allPaths, int pageIndex, Type viewType)
    {
        var pageItems = allPaths.Skip(pageIndex * _pageSize).Take(_pageSize).ToList();

        grid.Children.Clear();
        grid.RowDefinitions.Clear();
        grid.ColumnDefinitions.Clear();

        for (var column = 0; column < _columns; column++)
            grid.ColumnDefinitions.Add(new ColumnDefinition());

        var numberOfRows = (int)Math.Ceiling(pageItems.Count / (double)_columns);

        for (var row = 0; row < numberOfRows; row++)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            for (var column = 0; column < _columns; column++)
            {
                var index = row * _columns + column;
                if (index >= pageItems.Count)
                    continue;

                var itemPath = pageItems[index];
                var view = _viewFactory.Create(viewType, _actorName, itemPath);
                if (view is null)
                    continue;

                Grid.SetRow(view, row);
                Grid.SetColumn(view, column);
                grid.Children.Add(view);
            }
        }
    }
}
