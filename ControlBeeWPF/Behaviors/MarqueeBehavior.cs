using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Brushes = System.Windows.Media.Brushes;
using Button = System.Windows.Controls.Button;
using FlowDirection = System.Windows.FlowDirection;
using FontFamily = System.Windows.Media.FontFamily;
using FontStyle = System.Windows.FontStyle;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Label = System.Windows.Controls.Label;

namespace ControlBeeWPF.Behaviors;

public static class MarqueeBehavior
{
    public static double ScrollPixelsPerSecond { get; set; } = 40.0;
    public static double CycleDurationSeconds { get; set; } = 4.0;
    public static double PauseDurationSeconds { get; set; } = 1.0;
    public static double ScrollAnimationThresholdPixels { get; set; } = 5.0;

    private static readonly DependencyProperty IsProcessedProperty =
        DependencyProperty.RegisterAttached("IsProcessed", typeof(bool), typeof(MarqueeBehavior));

    private static readonly DependencyProperty ScrollTransformProperty =
        DependencyProperty.RegisterAttached(
            "ScrollTransform",
            typeof(TranslateTransform),
            typeof(MarqueeBehavior)
        );

    static MarqueeBehavior()
    {
        EventManager.RegisterClassHandler(
            typeof(Label),
            FrameworkElement.SizeChangedEvent,
            new RoutedEventHandler(OnContentControlSizeChanged),
            true
        );
        EventManager.RegisterClassHandler(
            typeof(Button),
            FrameworkElement.SizeChangedEvent,
            new RoutedEventHandler(OnContentControlSizeChanged),
            true
        );
        EventManager.RegisterClassHandler(
            typeof(TextBlock),
            FrameworkElement.SizeChangedEvent,
            new RoutedEventHandler(OnTextBlockSizeChanged),
            true
        );
    }

    public static bool Enabled { get; set; } = true;

    private static string[] _projectNamespacePrefixes = [];

    public static void Initialize(params string[] projectNamespacePrefixes)
    {
        _projectNamespacePrefixes = ["ControlBeeWPF", .. projectNamespacePrefixes];
    }

    private static void OnContentControlSizeChanged(object sender, RoutedEventArgs e)
    {
        if (
            sender is ContentControl control
            && !(bool)control.GetValue(IsProcessedProperty)
            && IsInProjectScope(control)
        )
            EvaluateContentControl(control);
    }

    private static void OnTextBlockSizeChanged(object sender, RoutedEventArgs e)
    {
        if (sender is not TextBlock textBlock)
            return;

        if ((bool)textBlock.GetValue(IsProcessedProperty))
            return;

        if (!IsInProjectScope(textBlock))
            return;

        if (IsInsideMarqueeTarget(textBlock))
            return;

        EvaluateTextBlock(textBlock);
    }

    private static void EvaluateContentControl(ContentControl control)
    {
        if (!Enabled || (bool)control.GetValue(IsProcessedProperty))
            return;

        var textWidth = MeasureTextWidth(control);
        if (textWidth == null)
            return;

        var availableWidth =
            control.ActualWidth
            - control.Padding.Left
            - control.Padding.Right
            - control.BorderThickness.Left
            - control.BorderThickness.Right;

        if (availableWidth <= 0)
            return;

        var overflowPixels = textWidth.Value - availableWidth;
        if (overflowPixels <= 0)
            return;

        var contentPresenter = FindChild<ContentPresenter>(control);
        if (contentPresenter == null)
            return;

        control.SetValue(IsProcessedProperty, true);
        control.HorizontalContentAlignment = HorizontalAlignment.Left;
        contentPresenter.Margin = new Thickness(0, 0, -overflowPixels, 0);

        if (overflowPixels > ScrollAnimationThresholdPixels)
        {
            control.ClipToBounds = true;
            StartScrollAnimation(control, contentPresenter, overflowPixels);
        }
    }

    private static void EvaluateTextBlock(TextBlock textBlock)
    {
        if (!Enabled || (bool)textBlock.GetValue(IsProcessedProperty))
            return;

        var textWidth = MeasureTextWidth(textBlock);
        if (textWidth == null)
            return;

        var availableWidth = textBlock.ActualWidth;
        if (availableWidth <= 0)
            return;

        var overflowPixels = textWidth.Value - availableWidth;
        if (overflowPixels <= ScrollAnimationThresholdPixels)
            return;

        var parent = VisualTreeHelper.GetParent(textBlock) as FrameworkElement;
        if (parent == null)
            return;

        textBlock.SetValue(IsProcessedProperty, true);
        parent.ClipToBounds = true;
        textBlock.Width = textWidth.Value;
        StartScrollAnimation(textBlock, textBlock, overflowPixels);
    }

    private static void StartScrollAnimation(
        FrameworkElement owner,
        FrameworkElement scrollTarget,
        double scrollDistance
    )
    {
        var transform = new TranslateTransform(0, 0);
        scrollTarget.RenderTransform = transform;
        owner.SetValue(ScrollTransformProperty, transform);

        var scrollWindowSeconds = CycleDurationSeconds - PauseDurationSeconds * 2;
        var actualScrollSeconds = Math.Min(
            scrollDistance / ScrollPixelsPerSecond,
            scrollWindowSeconds
        );
        var pauseDuration = TimeSpan.FromSeconds(PauseDurationSeconds);

        var animation = new DoubleAnimationUsingKeyFrames
        {
            RepeatBehavior = RepeatBehavior.Forever,
            Duration = new Duration(TimeSpan.FromSeconds(CycleDurationSeconds)),
        };

        var time = TimeSpan.Zero;
        animation.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromTimeSpan(time)));

        time += pauseDuration;
        animation.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromTimeSpan(time)));

        time += TimeSpan.FromSeconds(actualScrollSeconds);
        animation.KeyFrames.Add(
            new LinearDoubleKeyFrame(-scrollDistance, KeyTime.FromTimeSpan(time))
        );

        animation.KeyFrames.Add(
            new LinearDoubleKeyFrame(
                -scrollDistance,
                KeyTime.FromTimeSpan(TimeSpan.FromSeconds(CycleDurationSeconds))
            )
        );

        transform.BeginAnimation(TranslateTransform.XProperty, animation);
    }

    private static double? MeasureTextWidth(ContentControl control)
    {
        if (control.Content is not string text || string.IsNullOrEmpty(text))
            return null;

        return CreateFormattedText(
            text,
            control.FontFamily,
            control.FontStyle,
            control.FontWeight,
            control.FontStretch,
            control.FontSize,
            control.FlowDirection,
            control
        ).WidthIncludingTrailingWhitespace;
    }

    private static double? MeasureTextWidth(TextBlock textBlock)
    {
        if (string.IsNullOrEmpty(textBlock.Text))
            return null;

        return CreateFormattedText(
            textBlock.Text,
            textBlock.FontFamily,
            textBlock.FontStyle,
            textBlock.FontWeight,
            textBlock.FontStretch,
            textBlock.FontSize,
            textBlock.FlowDirection,
            textBlock
        ).WidthIncludingTrailingWhitespace;
    }

    private static FormattedText CreateFormattedText(
        string text,
        FontFamily fontFamily,
        FontStyle fontStyle,
        FontWeight fontWeight,
        FontStretch fontStretch,
        double fontSize,
        FlowDirection flowDirection,
        Visual visual
    )
    {
        return new FormattedText(
            text,
            CultureInfo.CurrentCulture,
            flowDirection,
            new Typeface(fontFamily, fontStyle, fontWeight, fontStretch),
            fontSize,
            Brushes.Black,
            VisualTreeHelper.GetDpi(visual).PixelsPerDip
        );
    }

    private static bool IsInProjectScope(FrameworkElement element)
    {
        var window = Window.GetWindow(element);
        if (window == null)
            return false;

        var ns = window.GetType().Namespace;
        return _projectNamespacePrefixes.Any(prefix => ns?.StartsWith(prefix) == true);
    }

    private static bool IsInsideMarqueeTarget(TextBlock textBlock)
    {
        DependencyObject current = VisualTreeHelper.GetParent(textBlock);
        while (current != null)
        {
            if (current is Label or Button)
                return true;
            current = VisualTreeHelper.GetParent(current);
        }

        return false;
    }

    private static T? FindChild<T>(DependencyObject parent)
        where T : DependencyObject
    {
        var count = VisualTreeHelper.GetChildrenCount(parent);
        for (var i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T found)
                return found;

            var result = FindChild<T>(child);
            if (result != null)
                return result;
        }

        return null;
    }
}
