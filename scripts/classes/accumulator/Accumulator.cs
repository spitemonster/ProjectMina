using Godot;

namespace ProjectMina;

public partial class Accumulator : Node
{
    [Signal] public delegate void ThresholdReachedEventHandler();
    [Signal] public delegate void ThresholdLostEventHandler();
    
    public float Threshold;
    public float SampleFrequency = .1f;
    public float DecreaseRate = 20f;
    public float Value;
    public bool ThresholdPassed { get; protected set; }

    private float _smoothValue;
    private float _v;

    private Timer _timer;

    public override void _Ready()
    {
        _timer = new()
        {
            WaitTime = SampleFrequency,
            Autostart = true,
            OneShot = false
        };

        _timer.Timeout += _Sample;
        GetTree().Root.AddChild(_timer);
        _timer.Start();
    }

    private void _Sample()
    {
        if (Value == 0)
        {
            _v -= DecreaseRate * SampleFrequency;
            _v = Mathf.Max(0, _v);
        }
        else
        {
            _v += Value;
        }
        
        _smoothValue = Mathf.Lerp(_smoothValue, _v, 1.0f);

        if (_smoothValue > Threshold)
        {
            if (!ThresholdPassed)
            {
                EmitSignal(SignalName.ThresholdReached);
                ThresholdPassed = true;
            }
        }
        else
        {
            if (ThresholdPassed)
            {
                EmitSignal(SignalName.ThresholdLost);
                ThresholdPassed = false;
            }
        }
    }
}