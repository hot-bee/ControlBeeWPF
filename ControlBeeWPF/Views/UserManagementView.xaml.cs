using System.Windows;
using ControlBeeWPF.ViewModels;

namespace ControlBeeWPF.Views;

/// <summary>
///     Interaction logic for UserManagementView.xaml
/// </summary>
public partial class UserManagementView : Window
{
    public UserManagementView(UserManagementViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}