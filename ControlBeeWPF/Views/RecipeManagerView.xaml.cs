using System.Windows;
using System.Windows.Controls;
using ControlBee.Interfaces;
using ControlBeeWPF.Components;
using ControlBeeWPF.ViewModels;
using MessageBox = System.Windows.MessageBox;
using UserControl = System.Windows.Controls.UserControl;

namespace ControlBeeWPF.Views;

/// <summary>
///     Interaction logic for RecipeManagerView.xaml
/// </summary>
public partial class RecipeManagerView : UserControl
{
    private readonly IVariableManager _variableManager;
    private readonly RecipeManagerViewModel _viewModel;

    public RecipeManagerView(RecipeManagerViewModel viewModel, IVariableManager variableManager)
    {
        DataContext = viewModel;
        _viewModel = viewModel;
        _variableManager = variableManager;
        InitializeComponent();
    }

    private void OpenButton_OnClick(object sender, RoutedEventArgs e)
    {
        var recipeName = RecipeListBox.SelectedItem as string;
        if (string.IsNullOrEmpty(recipeName))
            return;
        _variableManager.Load(recipeName);
    }

    private void SaveAsButton_OnClick(object sender, RoutedEventArgs e)
    {
        var inputBox = new InputBox();
        if (inputBox.ShowDialog() is not true)
            return;
        var recipeName = inputBox.ResponseText;
        if (string.IsNullOrEmpty(recipeName))
            return;
        _variableManager.Save(recipeName);
    }

    private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
    {
        var recipeName = RecipeListBox.SelectedItem as string;
        if (string.IsNullOrEmpty(recipeName))
            return;
        if (
            MessageBox.Show(
                $"Are you sure to DELETE \"{recipeName}\"?",
                "Delete",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Warning
            ) == MessageBoxResult.Yes
        )
            _variableManager.Delete(recipeName);
    }
}
