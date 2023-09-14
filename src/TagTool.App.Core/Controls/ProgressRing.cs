using Avalonia;
using Avalonia.Controls.Primitives;

namespace TagTool.App.Core.Controls;

public class ProgressRing : TemplatedControl
{
    private const string LargeState = ":large";
    private const string SmallState = ":small";

    private const string InactiveState = ":inactive";
    private const string ActiveState = ":active";

    public static readonly StyledProperty<bool> IsActiveProperty
        = AvaloniaProperty.Register<ProgressRing, bool>(
            nameof(IsActive),
            true);

    public static readonly DirectProperty<ProgressRing, double> MaxSideLengthProperty =
        AvaloniaProperty.RegisterDirect<ProgressRing, double>(
            nameof(MaxSideLength),
            o => o.MaxSideLength,
            (o, v) => o.MaxSideLength = v);

    public static readonly DirectProperty<ProgressRing, double> EllipseDiameterProperty =
        AvaloniaProperty.RegisterDirect<ProgressRing, double>(
            nameof(EllipseDiameter),
            o => o.EllipseDiameter,
            (o, v) => o.EllipseDiameter = v);

    public static readonly DirectProperty<ProgressRing, Thickness> EllipseOffsetProperty =
        AvaloniaProperty.RegisterDirect<ProgressRing, Thickness>(
            nameof(EllipseOffset),
            o => o.EllipseOffset,
            (o, v) => o.EllipseOffset = v);

    private double _maxSideLength = 10;
    private double _ellipseDiameter = 10;
    private Thickness _ellipseOffset = new(2);

    public bool IsActive
    {
        get => GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    public double MaxSideLength
    {
        get => _maxSideLength;
        private set => SetAndRaise(MaxSideLengthProperty, ref _maxSideLength, value);
    }

    public double EllipseDiameter
    {
        get => _ellipseDiameter;
        private set => SetAndRaise(EllipseDiameterProperty, ref _ellipseDiameter, value);
    }

    public Thickness EllipseOffset
    {
        get => _ellipseOffset;
        private set => SetAndRaise(EllipseOffsetProperty, ref _ellipseOffset, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        var maxSideLength = Math.Min(Width, Height);
        var ellipseDiameter = 0.1 * maxSideLength;
        if (maxSideLength <= 40)
        {
            ellipseDiameter++;
        }

        EllipseDiameter = ellipseDiameter;
        MaxSideLength = maxSideLength;
        EllipseOffset = new Thickness(0, (maxSideLength / 2) - ellipseDiameter, 0, 0);
        UpdateVisualStates();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsActiveProperty)
        {
            UpdateVisualStates();
        }
    }

    private void UpdateVisualStates()
    {
        PseudoClasses.Remove(ActiveState);
        PseudoClasses.Remove(InactiveState);
        PseudoClasses.Remove(SmallState);
        PseudoClasses.Remove(LargeState);
        PseudoClasses.Add(IsActive ? ActiveState : InactiveState);
        PseudoClasses.Add(_maxSideLength < 60 ? SmallState : LargeState);
    }
}
