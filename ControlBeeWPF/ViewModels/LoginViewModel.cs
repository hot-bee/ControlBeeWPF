using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ControlBee.Interfaces;
using Microsoft.VisualBasic.ApplicationServices;
using System.ComponentModel;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace ControlBeeWPF.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IUserInfo _userInfo;

    public LoginViewModel(IUserInfo userInfo)
    {
        _userInfo = userInfo;
    }

    [ObservableProperty]
    private string _userId;

    [ObservableProperty]
    private string _userPassword;

    [RelayCommand]
    private void Login()
    {
        if (string.IsNullOrWhiteSpace(UserId) || string.IsNullOrWhiteSpace(UserPassword))
        {
            MessageBox.Show("Please enter both your ID and password.", "Notice", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (_userInfo.ValidateUser(UserId, UserPassword))
        {
            MessageBox.Show($"Welcome, {_userInfo.Name}!", "Login Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            WeakReferenceMessenger.Default.Send(new LoginSuccessMessage());
        }
        else
            MessageBox.Show("Login failed: Invalid ID or password.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public sealed class LoginSuccessMessage { }
}
