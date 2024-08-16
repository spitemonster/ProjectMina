using Godot;

namespace ProjectMina;
[GlobalClass]
public partial class AIMovementState : CharacterMovementState
{
	protected AnimationNodeStateMachinePlayback CurrentAnimStateMachine;
	protected AICharacter aiCharacter;

	public override void _Ready()
	{
		base._Ready();

		aiCharacter = GetOwner<AICharacter>();

		if (aiCharacter == null)
		{
			GD.Print("NO AI CHARACTER");
		}
	}

	public override void SetupAnimComponents()
	{
		base.SetupAnimComponents();
		AnimStateMachineRoot = (AnimationNodeStateMachinePlayback)Character.AnimTree.Get("parameters/playback");
		GD.Print("state machine root: ", AnimStateMachineRoot);
	}
}
