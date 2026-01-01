using System.Windows;
using ControlBeeWPF.ViewModels;

namespace ControlBeeWPF.Views;

/// <summary>
/// Interaction logic for LoginView.xaml
/// </summary>
public partial class LoginView : Window
{
    public LoginView(LoginViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        viewModel.LoginSucceeded += (_, _) => DialogResult = true;
    }
}
