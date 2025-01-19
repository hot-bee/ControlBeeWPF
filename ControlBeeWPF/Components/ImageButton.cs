using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ControlBeeWPF.Components;

public class ImageButton : Button
{
    public ImageButton(BitmapImage image, string text)
    {
        var stackPanel = new StackPanel { Orientation = Orientation.Horizontal };
        stackPanel.Children.Add(
            new Image
            {
                Source = image,
                Width = 16,
                Height = 16,
            }
        );
        stackPanel.Children.Add(
            new TextBlock
            {
                Text = text,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(5, 0, 0, 0),
            }
        );
        Content = stackPanel;
    }
}
