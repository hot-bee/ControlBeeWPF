﻿using System.Windows;
using System.Windows.Controls;
using ControlBeeWPF.ViewModels;

namespace ControlBeeWPF.Views;

/// <summary>
///     Interaction logic for TeachingDataView.xaml
/// </summary>
public partial class TeachingDataView : UserControl, IDisposable
{
    public TeachingDataView(TeachingDataViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        MyDataGrid.AutoGeneratedColumns += MyDataGridOnAutoGeneratedColumns;
    }

    public void Dispose()
    {
        MyDataGrid.AutoGeneratedColumns -= MyDataGridOnAutoGeneratedColumns;
    }

    private void MyDataGridOnAutoGeneratedColumns(object? sender, EventArgs e)
    {
        for (var i = 1; i < MyDataGrid.Columns.Count; i++)
            if (MyDataGrid.Columns[i] is DataGridTextColumn column)
            {
                column.Binding.StringFormat = "N3";
                column.CellStyle = new Style
                {
                    Setters = { new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Right) },
                };
            }
    }
}
