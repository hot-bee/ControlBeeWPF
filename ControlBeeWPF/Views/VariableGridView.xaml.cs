using System.Windows;
using System.Windows.Controls;
using ControlBeeWPF.Interfaces;
using ControlBeeWPF.Models;
using ControlBeeWPF.ViewModels;
using Binding = System.Windows.Data.Binding;
using BindingMode = System.Windows.Data.BindingMode;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Label = System.Windows.Controls.Label;

namespace ControlBeeWPF.Views;

/// <summary>
///     Interaction logic for VariableGridView.xaml
/// </summary>
public partial class VariableGridView
{
    public static readonly DependencyProperty ItemWidthProperty = DependencyProperty.Register(
        nameof(ItemWidth),
        typeof(double),
        typeof(VariableGridView),
        new PropertyMetadata(90.0)
    );

    public static readonly DependencyProperty HeaderColumnWidthProperty =
        DependencyProperty.Register(
            nameof(HeaderColumnWidth),
            typeof(double),
            typeof(VariableGridView),
            new PropertyMetadata(double.NaN)
        );

    public static readonly DependencyProperty FooterColumnWidthProperty =
        DependencyProperty.Register(
            nameof(FooterColumnWidth),
            typeof(double),
            typeof(VariableGridView),
            new PropertyMetadata(double.NaN)
        );

    public double ItemWidth
    {
        get => (double)GetValue(ItemWidthProperty);
        set => SetValue(ItemWidthProperty, value);
    }

    public double HeaderColumnWidth
    {
        get => (double)GetValue(HeaderColumnWidthProperty);
        set => SetValue(HeaderColumnWidthProperty, value);
    }

    public double FooterColumnWidth
    {
        get => (double)GetValue(FooterColumnWidthProperty);
        set => SetValue(FooterColumnWidthProperty, value);
    }

    private readonly VariableGridViewModel _viewModel;
    private readonly IViewFactory _viewFactory;
    private readonly int _rowCount;
    private readonly int _colCount;
    private bool _useCellName = true;
    private string? _title;
    private string[]? _headerColumns;
    private string[]? _headerRows;
    private string[]? _footerColumns;
    private string[]? _footerRows;
    private bool _rendered;

    private bool IsTableMode =>
        _title != null
        || _headerColumns != null
        || _headerRows != null
        || _footerColumns != null
        || _footerRows != null;

    public VariableGridView(
        IViewFactory viewFactory,
        VariableGridViewModel viewModel,
        int rowCount,
        int colCount
    )
    {
        InitializeComponent();
        _viewFactory = viewFactory;
        DataContext = viewModel;
        _viewModel = viewModel;
        _rowCount = rowCount;
        _colCount = colCount;

        Loaded += OnLoaded;
    }

    public VariableGridView UseCellName(bool value)
    {
        _useCellName = value;
        return this;
    }

    public VariableGridView SetTitle(string title)
    {
        _title = title;
        return this;
    }

    public VariableGridView SetHeaderColumns(string[] values)
    {
        _headerColumns = values;
        return this;
    }

    public VariableGridView SetHeaderRows(string[] values)
    {
        _headerRows = values;
        return this;
    }

    public VariableGridView SetFooterColumns(string[] values)
    {
        _footerColumns = values;
        return this;
    }

    public VariableGridView SetFooterRows(string[] values)
    {
        _footerRows = values;
        return this;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_rendered)
            return;

        RenderTable();
        _rendered = true;
    }

    public void AddVariable(int row, int col, ActorItemKey actorItemKey, string? cellName = null)
    {
        _viewModel.AddVariable(row, col, actorItemKey, cellName);
    }

    private int _totalRows;
    private int _totalCols;

    private void RenderTable()
    {
        var titleOffset = _title != null ? 1 : 0;
        var colOffset = _headerColumns != null ? 1 : 0;
        var headerRowOffset = titleOffset + (_headerRows != null ? 1 : 0);
        _totalRows = headerRowOffset + _rowCount + (_footerRows != null ? 1 : 0);
        _totalCols = colOffset + _colCount + (_footerColumns != null ? 1 : 0);

        for (var row = 0; row < _totalRows; row++)
            CellGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        if (_headerColumns != null)
            CellGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var columnWidth = IsTableMode ? GridLength.Auto : new GridLength(1, GridUnitType.Star);
        for (var col = 0; col < _colCount; col++)
            CellGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = columnWidth });

        if (_footerColumns != null)
            CellGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        RenderTitle();
        RenderHeaderRows(titleOffset);
        RenderHeaderColumns(headerRowOffset);
        RenderCells(headerRowOffset, colOffset);
        RenderFooterColumns(headerRowOffset, colOffset);
        RenderFooterRows(headerRowOffset, colOffset);
    }

    private Thickness GetTableMargin(int gridRow, int gridCol)
    {
        var isLastRow = gridRow == _totalRows - 1;
        var isLastCol = gridCol == _totalCols - 1;
        return new Thickness(0, 0, isLastCol ? 0 : -1, isLastRow ? 0 : -1);
    }

    private void RenderTitle()
    {
        if (_title == null)
            return;

        var label = CreateHeaderLabel(_title);
        label.Width = double.NaN;
        label.HorizontalAlignment = HorizontalAlignment.Stretch;
        label.Margin = GetTableMargin(0, _totalCols - 1);
        Grid.SetRow(label, 0);
        Grid.SetColumn(label, 0);
        Grid.SetColumnSpan(label, _totalCols);
        CellGrid.Children.Add(label);
    }

    private void RenderHeaderRows(int titleOffset)
    {
        if (_headerRows == null)
            return;

        var lastCol = _totalCols - 1;
        for (var i = 0; i < _headerRows.Length && i < _totalCols; i++)
        {
            var isHeaderCol = _headerColumns != null && i == 0;
            var isFooterCol = _footerColumns != null && i == lastCol;
            Label label;
            if (isHeaderCol)
                label = CreateHeaderColumnLabel(_headerRows[i]);
            else if (isFooterCol)
                label = CreateFooterColumnLabel(_headerRows[i]);
            else
                label = CreateHeaderLabel(_headerRows[i]);
            label.Margin = GetTableMargin(titleOffset, i);
            Grid.SetRow(label, titleOffset);
            Grid.SetColumn(label, i);
            CellGrid.Children.Add(label);
        }
    }

    private void RenderHeaderColumns(int rowOffset)
    {
        if (_headerColumns == null)
            return;

        for (var i = 0; i < _headerColumns.Length && i < _rowCount; i++)
        {
            var gridRow = rowOffset + i;
            var label = CreateHeaderColumnLabel(_headerColumns[i]);
            label.Margin = GetTableMargin(gridRow, 0);
            Grid.SetRow(label, gridRow);
            Grid.SetColumn(label, 0);
            CellGrid.Children.Add(label);
        }
    }

    private void RenderCells(int rowOffset, int colOffset)
    {
        foreach (var cell in _viewModel.Cells)
        {
            if (_useCellName)
            {
                var container = CreateCellWithName(cell);
                Grid.SetRow(container, rowOffset + cell.Row);
                Grid.SetColumn(container, colOffset + cell.Col);
                CellGrid.Children.Add(container);
            }
            else
            {
                var view = _viewFactory.Create<VariableItemView>(cell.ViewModel);
                if (view == null)
                    continue;

                if (IsTableMode)
                {
                    var gridRow = rowOffset + cell.Row;
                    var gridCol = colOffset + cell.Col;
                    view.CornerRadius = new CornerRadius(0);
                    view.BorderMargin = new Thickness(0);
                    view.Margin = GetTableMargin(gridRow, gridCol);
                }

                Grid.SetRow(view, rowOffset + cell.Row);
                Grid.SetColumn(view, colOffset + cell.Col);
                CellGrid.Children.Add(view);
            }
        }
    }

    private void RenderFooterRows(int rowOffset, int colOffset)
    {
        if (_footerRows == null)
            return;

        var footerRowIndex = rowOffset + _rowCount;
        var lastCol = _totalCols - 1;
        for (var i = 0; i < _footerRows.Length && i < _totalCols; i++)
        {
            var isHeaderCol = _headerColumns != null && i == 0;
            var isFooterCol = _footerColumns != null && i == lastCol;
            Label label;
            if (isHeaderCol)
                label = CreateHeaderColumnLabel(_footerRows[i]);
            else if (isFooterCol)
                label = CreateFooterColumnLabel(_footerRows[i]);
            else
                label = CreateHeaderLabel(_footerRows[i]);
            label.Margin = GetTableMargin(footerRowIndex, i);
            Grid.SetRow(label, footerRowIndex);
            Grid.SetColumn(label, i);
            CellGrid.Children.Add(label);
        }
    }

    private void RenderFooterColumns(int rowOffset, int colOffset)
    {
        if (_footerColumns == null)
            return;

        var footerColIndex = colOffset + _colCount;
        for (var i = 0; i < _footerColumns.Length && i < _rowCount; i++)
        {
            var gridRow = rowOffset + i;
            var label = CreateFooterColumnLabel(_footerColumns[i]);
            label.Margin = GetTableMargin(gridRow, footerColIndex);
            Grid.SetRow(label, gridRow);
            Grid.SetColumn(label, footerColIndex);
            CellGrid.Children.Add(label);
        }
    }

    private Grid CreateCellWithName(VariableGridViewModel.Cell cell)
    {
        var container = new Grid
        {
            Margin = new Thickness(2),
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        container.ColumnDefinitions.Add(
            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
        );
        container.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var label = new Label
        {
            Content = cell.CellName,
            Style = (Style)FindResource("MainLabelStyle"),
            MinWidth = 120,
        };
        label.SetBinding(
            ContentProperty,
            new Binding(nameof(VariableGridViewModel.Cell.CellName))
            {
                Source = cell,
                Mode = BindingMode.OneWay,
            }
        );
        Grid.SetColumn(label, 0);
        container.Children.Add(label);

        var view = _viewFactory.Create<VariableItemView>(cell.ViewModel);
        if (view == null)
            return container;
        view.CornerRadius = new CornerRadius(0, 12, 12, 0);
        view.BorderMargin = new Thickness(0);
        Grid.SetColumn(view, 1);
        container.Children.Add(view);

        return container;
    }

    private Label CreateHeaderLabel(string text)
    {
        return new Label { Content = text, Style = (Style)FindResource("HeaderLabelStyle") };
    }

    private Label CreateHeaderColumnLabel(string text)
    {
        var label = new Label { Content = text, Style = (Style)FindResource("HeaderLabelStyle") };
        label.SetBinding(WidthProperty, new Binding(nameof(HeaderColumnWidth)) { Source = this });
        return label;
    }

    private Label CreateFooterColumnLabel(string text)
    {
        var label = new Label { Content = text, Style = (Style)FindResource("HeaderLabelStyle") };
        label.SetBinding(WidthProperty, new Binding(nameof(FooterColumnWidth)) { Source = this });
        return label;
    }
}
