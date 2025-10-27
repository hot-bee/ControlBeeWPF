using CommunityToolkit.Mvvm.Messaging;
using ControlBeeWPF.ViewModels;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using static ControlBeeWPF.ViewModels.LoginViewModel;

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

        WeakReferenceMessenger.Default.Register<LoginSuccessMessage>(this, (_, __) =>
        {
            DialogResult = true;
        });
    }
}
