using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ControlBee.Interfaces;
using ControlBee.Models;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;

namespace ControlBeeWPF.Views;

public partial class DoubleActingActuatorStatusBarViewV2 : IDisposable
{
    public static readonly DependencyProperty NameColumnWidthProperty = DependencyProperty.Register(
        nameof(NameColumnWidth),
        typeof(GridLength),
        typeof(DoubleActingActuatorStatusBarViewV2),
        new PropertyMetadata(new GridLength(7, GridUnitType.Star))
    );

    public static readonly DependencyProperty RowHeightProperty = DependencyProperty.Register(
        nameof(RowHeight),
        typeof(double),
        typeof(DoubleActingActuatorStatusBarViewV2),
        new PropertyMetadata(30.0)
    );

    private static readonly SolidColorBrush DetectOffBrush = new(Color.FromRgb(0xE8, 0xE8, 0xE8));

    private readonly IActor _actor;
    private readonly ActorItemBinder _binder;
    private readonly string _itemPath;
    private readonly IActor _uiActor;
    private bool _commandOn;

    public DoubleActingActuatorStatusBarViewV2(
        IActorRegistry actorRegistry,
        string actorName,
        string itemPath
    )
    {
        InitializeComponent();
        _itemPath = itemPath;
        _actor = actorRegistry.Get(actorName)!;
        _uiActor = actorRegistry.Get("Ui")!;
        _binder = new ActorItemBinder(actorRegistry, actorName, itemPath);
        _binder.MetaDataChanged += BinderOnMetaDataChanged;
        _binder.DataChanged += BinderOnDataChanged;
    }

    public GridLength NameColumnWidth
    {
        get => (GridLength)GetValue(NameColumnWidthProperty);
        set => SetValue(NameColumnWidthProperty, value);
    }

    public double RowHeight
    {
        get => (double)GetValue(RowHeightProperty);
        set => SetValue(RowHeightProperty, value);
    }

    public void Dispose()
    {
        _binder.MetaDataChanged -= BinderOnMetaDataChanged;
        _binder.DataChanged -= BinderOnDataChanged;
        _binder.Dispose();
    }

    private void BinderOnMetaDataChanged(object? sender, Dictionary<string, object?> e)
    {
        var desc = e["Desc"] as string;
        NameText.Text = e["Name"] as string ?? string.Empty;
        ToolTip = string.IsNullOrEmpty(desc) ? NameText.Text : desc;
    }

    private void BinderOnDataChanged(object? sender, Dictionary<string, object?> e)
    {
        _commandOn = e["CommandOn"] is true;
        UpdateDetect(OnDetectBorder, OnDetectText, e["OnDetect"] is true);
        UpdateDetect(OffDetectBorder, OffDetectText, e["OffDetect"] is true);
        ValueText.Text = _commandOn ? "On" : "Off";
        ValueBorder.Background = _commandOn ? Brushes.LawnGreen : Brushes.LightGray;
        ValueText.Foreground = _commandOn ? Brushes.Black : Brushes.DimGray;
    }

    private static void UpdateDetect(Border border, TextBlock text, bool detected)
    {
        border.Background = detected ? Brushes.OrangeRed : DetectOffBrush;
        text.Foreground = detected ? Brushes.White : Brushes.DimGray;
    }

    private void ValueBorder_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _actor.Send(
            new ActorItemMessage(
                _uiActor,
                _itemPath,
                "_itemDataWrite",
                new Dictionary<string, object?> { ["On"] = !_commandOn }
            )
        );
    }
}
