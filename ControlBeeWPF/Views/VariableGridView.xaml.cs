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

    public static readonly DependencyProperty FooterRowHeightProperty = DependencyProperty.Register(
        nameof(FooterRowHeight),
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

    public double FooterRowHeight
    {
        get => (double)GetValue(FooterRowHeightProperty);
        set => SetValue(FooterRowHeightProperty, value);
    }

    private readonly VariableGridViewModel _viewModel;
    private readonly IViewFactory _viewFactory;
    private readonly int _rowCount;
    private readonly int _colCount;
    private bool _useCellName = true;
    private string? _cornerLabel;
    private string[]? _headerColumns;
    private string[]? _headerRows;
    private string[]? _footerColumns;
    private string[]? _footerRows;
    private bool _rendered;

    private bool IsTableMode => _headerColumns != null || _headerRows != null;

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

    public VariableGridView SetCornerLabel(string text)
    {
        _cornerLabel = text;
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
        var dataColOffset = _headerRows != null ? 1 : 0;
        var dataRowOffset = _headerColumns != null ? 1 : 0;
        _totalRows = dataRowOffset + _rowCount + (_footerColumns != null ? 1 : 0);
        _totalCols = dataColOffset + _colCount + (_footerRows != null ? 1 : 0);

        if (_headerColumns != null)
            CellGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        for (var row = 0; row < _rowCount; row++)
            CellGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        if (_footerColumns != null)
            CellGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        if (_headerRows != null)
            CellGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var columnWidth = IsTableMode ? GridLength.Auto : new GridLength(1, GridUnitType.Star);
        for (var col = 0; col < _colCount; col++)
            CellGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = columnWidth });
        if (_footerRows != null)
            CellGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        RenderHeaderColumns();
        RenderHeaderRows();
        if (_headerColumns != null && _headerRows != null)
            RenderCornerCell(0, 0, _cornerLabel ?? string.Empty);
        if (_footerColumns != null && _headerRows != null)
            RenderCornerCell(dataRowOffset + _rowCount, 0);
        if (_headerColumns != null && _footerRows != null)
            RenderCornerCell(dataRowOffset - 1, dataColOffset + _colCount);
        if (_footerColumns != null && _footerRows != null)
            RenderCornerCell(dataRowOffset + _rowCount, dataColOffset + _colCount);
        RenderCells(dataRowOffset, dataColOffset);
        RenderFooterRows();
        RenderFooterColumns();
    }

    private Thickness GetTableMargin(int gridRow, int gridCol)
    {
        var isLastRow = gridRow == _totalRows - 1;
        var isLastCol = gridCol == _totalCols - 1;
        return new Thickness(0, 0, isLastCol ? 0 : -1, isLastRow ? 0 : -1);
    }

    private void RenderCornerCell(int gridRow, int gridCol, string text = "")
    {
        var corner = CreateHeaderLabel(text);
        corner.Margin = GetTableMargin(gridRow, gridCol);
        Grid.SetRow(corner, gridRow);
        Grid.SetColumn(corner, gridCol);
        CellGrid.Children.Add(corner);
    }

    private void RenderHeaderColumns()
    {
        if (_headerColumns == null)
            return;

        var colOffset = _headerRows != null ? 1 : 0;
        var lastCol = _totalCols - 1;

        for (var i = 0; i < _headerColumns.Length; i++)
        {
            var gridCol = colOffset + i;
            if (gridCol >= _totalCols)
                break;
            var isFooterCol = _footerRows != null && gridCol == lastCol;
            var label = isFooterCol
                ? CreateFooterColumnLabel(_headerColumns[i])
                : CreateHeaderLabel(_headerColumns[i]);
            label.Margin = GetTableMargin(0, gridCol);
            Grid.SetRow(label, 0);
            Grid.SetColumn(label, gridCol);
            CellGrid.Children.Add(label);
        }
    }

    private void RenderHeaderRows()
    {
        if (_headerRows == null)
            return;

        var rowOffset = _headerColumns != null ? 1 : 0;
        var lastRow = _totalRows - 1;

        for (var i = 0; i < _headerRows.Length; i++)
        {
            var gridRow = rowOffset + i;
            if (gridRow >= _totalRows)
                break;
            var isFooterRow = _footerColumns != null && gridRow == lastRow;
            var label = isFooterRow
                ? CreateFooterRowLabel(_headerRows[i])
                : CreateHeaderColumnLabel(_headerRows[i]);
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

    private void RenderFooterColumns()
    {
        if (_footerColumns == null)
            return;

        var rowOffset = _headerColumns != null ? 1 : 0;
        var colOffset = _headerRows != null ? 1 : 0;
        var footerRowIndex = rowOffset + _rowCount;
        var lastCol = _totalCols - 1;

        for (var i = 0; i < _footerColumns.Length; i++)
        {
            var gridCol = colOffset + i;
            if (gridCol >= _totalCols)
                break;
            var isFooterCol = _footerRows != null && gridCol == lastCol;
            var label = isFooterCol
                ? CreateFooterColumnLabel(_footerColumns[i])
                : CreateHeaderLabel(_footerColumns[i]);
            label.Margin = GetTableMargin(footerRowIndex, gridCol);
            Grid.SetRow(label, footerRowIndex);
            Grid.SetColumn(label, gridCol);
            CellGrid.Children.Add(label);
        }
    }

    private void RenderFooterRows()
    {
        if (_footerRows == null)
            return;

        var rowOffset = _headerColumns != null ? 1 : 0;
        var colOffset = _headerRows != null ? 1 : 0;
        var footerColIndex = colOffset + _colCount;
        var lastRow = _totalRows - 1;

        for (var i = 0; i < _footerRows.Length; i++)
        {
            var gridRow = rowOffset + i;
            if (gridRow >= _totalRows)
                break;
            var isFooterRow = _footerColumns != null && gridRow == lastRow;
            var label = isFooterRow
                ? CreateFooterRowLabel(_footerRows[i])
                : CreateFooterColumnLabel(_footerRows[i]);
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

    private Label CreateFooterRowLabel(string text)
    {
        var label = new Label { Content = text, Style = (Style)FindResource("HeaderLabelStyle") };
        label.SetBinding(HeightProperty, new Binding(nameof(FooterRowHeight)) { Source = this });
        return label;
    }
}
