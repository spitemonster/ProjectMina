using Godot;

namespace ProjectMina;

[GlobalClass]
public partial class VisibilityComponent : ComponentBase3D
{
    // this functions as the component that calculates a character's visibility, or how well lit they are
    // there is a camera pointing at an object that only it can see but that can receive shadows from the environment
    // on update (set by VisibilityTimerRate) 

    // frequency at which the character's visibility is updated
    [Export] protected float VisibilityTimerRate = .1f;
    [Export] protected Camera3D Camera;
    [Export] protected SubViewport Viewport;

    [Export] protected Curve VisibilityCurve;
    
    [ExportCategory("Debug")]
    [Export] protected ColorRect RoughColor;
    [Export] protected ColorRect SmoothColor;
    [Export] protected ColorRect VisibilityRect;

    private Timer _visibilityTimer;

    private float _roughVisibility = 0;
    private float _smoothVisibility = 0;

    // private PlayerCharacter _owner;
    private double _visibility = 1.0;

    public float GetVisibility()
    {
        return VisibilityCurve?.SampleBaked(_smoothVisibility) ?? _smoothVisibility;
    }

    public override void _Ready()
    {
        _visibilityTimer = new()
        {
            WaitTime = VisibilityTimerRate,
            Autostart = true,
            OneShot = false
        };
        GetTree().Root.AddChild(_visibilityTimer);

        _visibilityTimer.Timeout += _UpdateVisibility;
        _visibilityTimer.Start();
    }

    public override void _PhysicsProcess(double delta)
    {
        Camera.GlobalPosition = GlobalPosition;
        Camera.GlobalRotation = Camera.GlobalRotation with { Y = GlobalRotation.Y };
        _smoothVisibility = Mathf.Lerp(_smoothVisibility, _roughVisibility, (float)(5.0 * delta));

        if (VisibilityRect != null)
        {
            Color c = VisibilityRect.Color;
            c.V = GetVisibility();
            VisibilityRect.Color = c;
        }
    }

    private void _UpdateVisibility()
    {
        var img = Viewport.GetTexture().GetImage();
        img.Resize(1, 1);

        var pixel = img.GetPixel(0, 0);

        _roughVisibility = pixel.Luminance;

        if (RoughColor != null)
        {
            RoughColor.Color = pixel;
        }
    }
}