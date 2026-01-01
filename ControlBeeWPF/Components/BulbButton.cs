using System.Windows;
using System.Windows.Controls;
using Button = System.Windows.Controls.Button;

namespace ControlBeeWPF.Components;

public class BulbButton : Button
{
    public static readonly DependencyProperty IsBulbOnProperty = DependencyProperty.Register(
        nameof(IsBulbOn),
        typeof(bool),
        typeof(BulbButton),
        new PropertyMetadata(false)
    );

    static BulbButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(BulbButton),
            new FrameworkPropertyMetadata(typeof(BulbButton))
        );
    }

    public bool IsBulbOn
    {
        get => (bool)GetValue(IsBulbOnProperty);
        set => SetValue(IsBulbOnProperty, value);
    }
}
