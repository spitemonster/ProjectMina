using Godot;
using System.Threading.Tasks;

namespace ProjectMina.BehaviorTree;

public partial class FindRandomTargetPositionInRadius : Action
{
	[Export] public float Radius { get; protected set; } = 10.0f;

	// TODO: move this to a global
	private RandomNumberGenerator rng = new();
	private Vector3 _newTarget;

	protected override async Task<ActionStatus> _Tick(AICharacter character, BlackboardComponent blackboard)
	{
		await Task.Run(() =>
		{

			CallDeferred("FindPosition", character, blackboard);
		});
		Succeed();
		return Status;
	}

	private void FindPosition(AICharacter character, BlackboardComponent blackboard)
	{
		var characterPosition = character.GlobalPosition;
		var newPosition = characterPosition;

		var x = characterPosition.X + rng.RandfRange(-12.5f, 12.5f);
		var z = characterPosition.Z + rng.RandfRange(-12.5f, 12.5f);

		newPosition.X = x;
		newPosition.Z = z;
		newPosition.Y = characterPosition.Y;

		Vector3 pos = NavigationServer3D.MapGetClosestPoint(character.Brain.NavigationAgent.GetNavigationMap(), newPosition);

		_newTarget = pos;
		blackboard.SetValue("target_movement_position", _newTarget);
		blackboard.SetValue("target_movement_position_reached", false);

	}

}
