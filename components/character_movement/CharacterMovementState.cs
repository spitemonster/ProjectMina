using Godot;

namespace ProjectMina;

[Tool]
[GlobalClass]
public partial class CharacterMovementState : State
{
	protected CharacterBase Character;

	public AnimationTree AnimTree;
	public AnimationNodeBlendTree AnimTreeRoot;
	public AnimationNodeStateMachinePlayback AnimStateMachine;
	
	public CharacterMovementStateMachine MovementStateMachine;
	
	public override void _Ready()
	{
		base._Ready();
		
		if (Machine is CharacterMovementStateMachine m)
		{
			MovementStateMachine = m;
		}
	}

	public virtual void SetupAnimComponents()
	{
		AnimTree = Character.AnimTree;
		AnimTreeRoot = (AnimationNodeBlendTree)AnimTree.GetTreeRoot();
		AnimStateMachine = (AnimationNodeStateMachinePlayback)Character.AnimTree.Get("parameters/movement_anims/playback");
	}
	
	public virtual bool Setup(CharacterBase character)
	{
		if (Character != null)
		{
			return false;
		}

		Character = character;
		SetupAnimComponents();
		return true;
	}

	public virtual Vector3 TickMovement(Vector2 movementInput, double delta)
	{
		base.PhysicsTick(delta);

		return new(movementInput.X, 0, movementInput.Y);
	}
}

