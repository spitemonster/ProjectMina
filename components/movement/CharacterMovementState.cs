using Godot;
namespace ProjectMina;

public partial class CharacterMovementState : State
{
    private MovementComponent _movementComponent;
    public virtual void Enter(MovementComponent movementComponent)
    {
        _movementComponent = movementComponent;
    }

    public virtual void Run()
    {
        
    }

    public virtual void Exit()
    {
        
    }

    public virtual void Setup()
    {
        
    }
}