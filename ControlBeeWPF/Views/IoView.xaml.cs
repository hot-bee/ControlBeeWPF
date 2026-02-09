using System.Windows;
using System.Windows.Controls;
using ControlBee.Interfaces;
using ControlBeeWPF.Interfaces;
using Button = System.Windows.Controls.Button;

namespace ControlBeeWPF.Views;

/// <summary>
///     Interaction logic for IoView.xaml
/// </summary>
public partial class IoView
{
    private readonly string _actorName;
    private readonly int _columns;

    private readonly List<string> _inputPaths = [];
    private readonly List<string> _outputPaths = [];
    private readonly int _pageSize;
    private readonly int _maxVisiblePages;
    private readonly IViewFactory _viewFactory;

    private int _inputPageIndex;
    private int _outputPageIndex;

    public IoView(
        string actorName,
        int columns,
        IActorRegistry actorRegistry,
        IViewFactory viewFactory,
        int? pageSize = 32,
        int? maxVisiblePages = 5
    )
    {
        _actorName = actorName;
        _columns = columns;
        _pageSize = pageSize ?? 32;
        _maxVisiblePages = maxVisiblePages ?? 5;
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

    private int GetTotalPages(int count)
    {
        return Math.Max(1, (int)Math.Ceiling(count / (double)_pageSize));
    }

    private void GoToInputPage(int page)
    {
        _inputPageIndex = ClampPage(page, GetTotalPages(_inputPaths.Count));
        RenderPage(InputGrid, _inputPaths, _inputPageIndex, typeof(DigitalInputStatusBarView));
        RefreshInputPager();
    }

    private void GoToOutputPage(int page)
    {
        _outputPageIndex = ClampPage(page, GetTotalPages(_outputPaths.Count));
        RenderPage(OutputGrid, _outputPaths, _outputPageIndex, typeof(DigitalOutputStatusBarView));
        RefreshOutputPager();
    }

    private static int ClampPage(int page, int total)
    {
        return Math.Max(0, Math.Min(page, total - 1));
    }

    private void RefreshInputPager()
    {
        var total = GetTotalPages(_inputPaths.Count);

        InputFirstButton.IsEnabled = _inputPageIndex > 0;
        InputPrevButton.IsEnabled = _inputPageIndex > 0;
        InputNextButton.IsEnabled = _inputPageIndex < total - 1;
        InputLastButton.IsEnabled = _inputPageIndex < total - 1;

        BuildPagerWindowed(InputPager, total, _inputPageIndex, GoToInputPage);
    }

    private void RefreshOutputPager()
    {
        var total = GetTotalPages(_outputPaths.Count);

        OutputFirstButton.IsEnabled = _outputPageIndex > 0;
        OutputPrevButton.IsEnabled = _outputPageIndex > 0;
        OutputNextButton.IsEnabled = _outputPageIndex < total - 1;
        OutputLastButton.IsEnabled = _outputPageIndex < total - 1;

        BuildPagerWindowed(OutputPager, total, _outputPageIndex, GoToOutputPage);
    }

    private void BuildPagerWindowed(
        WrapPanel host,
        int totalPages,
        int current,
        Action<int> onClick
    )
    {
        host.Children.Clear();
        totalPages = Math.Max(1, totalPages);

        if (totalPages <= _maxVisiblePages)
        {
            for (var i = 0; i < totalPages; i++)
                AddPage(i, i == current);
            return;
        }

        AddPage(0, current == 0);

        if (current <= 2)
        {
            AddPage(1, current == 1);
            AddPage(2, current == 2);
            AddDots();
        }
        else if (current >= totalPages - 3)
        {
            AddDots();
            AddPage(totalPages - 3, current == totalPages - 3);
            AddPage(totalPages - 2, current == totalPages - 2);
        }
        else
        {
            AddDots();
            AddPage(current, true);
            AddDots();
        }

        AddPage(totalPages - 1, current == totalPages - 1);
        return;

        void AddDots()
        {
            host.Children.Add(
                new Button
                {
                    Content = "...",
                    IsEnabled = false,
                    Style = (Style)FindResource("PagerButtonStyle"),
                }
            );
        }

        void AddPage(int index, bool active)
        {
            var style = (Style)FindResource(active ? "PagerButtonActiveStyle" : "PagerButtonStyle");

            var button = new Button
            {
                Content = (index + 1).ToString(),
                Style = style,
                IsEnabled = !active,
            };

            button.Click += (_, __) => onClick(index);
            host.Children.Add(button);
        }
    }

    private void RenderPage(Grid grid, List<string> paths, int pageIndex, Type viewType)
    {
        var items = paths.Skip(pageIndex * _pageSize).Take(_pageSize).ToList();

        grid.Children.Clear();
        grid.RowDefinitions.Clear();
        grid.ColumnDefinitions.Clear();

        for (var column = 0; column < _columns; column++)
            grid.ColumnDefinitions.Add(new ColumnDefinition());

        var rows = (int)Math.Ceiling(items.Count / (double)_columns);

        for (var row = 0; row < rows; row++)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            for (var column = 0; column < _columns; column++)
            {
                var index = row * _columns + column;
                if (index >= items.Count)
                    continue;

                var view = _viewFactory.Create(viewType, _actorName, items[index]);
                if (view == null)
                    continue;

                Grid.SetRow(view, row);
                Grid.SetColumn(view, column);
                grid.Children.Add(view);
            }
        }
    }
}
