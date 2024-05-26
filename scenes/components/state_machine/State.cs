using Godot;

namespace ProjectMina;

public partial class State : ComponentBase
{
    public bool Completed { get; protected set; }

    public double RunTime => Time.GetUnixTimeFromSystem() - _startTime;

    private double _startTime;
    
    public virtual void Enter()
    {
        _startTime = Time.GetUnixTimeFromSystem();
    }

    public virtual void Tick()
    {
        
    }

    public virtual void Exit()
    {
        
    }
}