using System.Windows;
using System.Windows.Controls;
using ControlBee.Interfaces;
using ControlBeeWPF.Interfaces;
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
    private readonly IViewFactory _viewFactory;
    private readonly RecipeManagerViewModel _viewModel;

    public RecipeManagerView(
        RecipeManagerViewModel viewModel,
        IVariableManager variableManager,
        IViewFactory viewFactory
    )
    {
        DataContext = viewModel;
        _viewModel = viewModel;
        _variableManager = variableManager;
        _viewFactory = viewFactory;
        InitializeComponent();
    }

    private void OpenButton_OnClick(object sender, RoutedEventArgs e)
    {
        var recipeName = RecipeListBox.SelectedItem as string;
        if (string.IsNullOrEmpty(recipeName))
            return;
        _variableManager.Load(recipeName);
        MessageBox.Show(
            $"Recipe \"{recipeName}\" has been loaded.",
            "Open",
            MessageBoxButton.OK,
            MessageBoxImage.Information
        );
    }

    private void SaveAsButton_OnClick(object sender, RoutedEventArgs e)
    {
        var keyboardView = _viewFactory.Create<KeyboardView>()!;
        if (keyboardView.ShowDialog() is not true)
            return;
        var recipeName = keyboardView.Value;
        if (string.IsNullOrEmpty(recipeName))
            return;
        if (_variableManager.LocalNames.Contains(recipeName))
        {
            MessageBox.Show(
                $"Recipe \"{recipeName}\" already exists.",
                "Save As",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
            return;
        }
        _variableManager.Save(recipeName);
    }

    private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
    {
        var recipeName = RecipeListBox.SelectedItem as string;
        if (string.IsNullOrEmpty(recipeName))
            return;
        if (recipeName == _variableManager.LocalName)
        {
            MessageBox.Show(
                "Cannot delete the currently loaded recipe.",
                "Delete",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
            return;
        }
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
