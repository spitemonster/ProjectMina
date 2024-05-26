using Godot;
using System.Threading.Tasks;

namespace ProjectMina.BehaviorTree;

public partial class FindRandomTargetPositionInRadius : Action
{
	[Export] public float Radius { get; protected set; } = 10.0f;

	// TODO: move this to a global
	private RandomNumberGenerator rng = new();
	private Vector3 _newTarget;

	protected override async Task<ActionStatus> _Tick(AgentComponent agent, BlackboardComponent blackboard)
	{
		await Task.Run(() =>
		{
			CallDeferred("FindPosition", agent, blackboard);
		});
		Succeed();
		return Status;
	}

	private void FindPosition(AgentComponent agent, BlackboardComponent blackboard)
	{
		GD.Print("FINDING POSITION");
		var characterPosition = agent.Pawn.GlobalPosition;
		var newPosition = characterPosition;

		var x = characterPosition.X + rng.RandfRange(-12.5f, 12.5f);
		var z = characterPosition.Z + rng.RandfRange(-12.5f, 12.5f);

		newPosition.X = x;
		newPosition.Z = z;
		newPosition.Y = characterPosition.Y;

		Vector3 pos = NavigationServer3D.MapGetClosestPoint(agent.Pawn.NavigationAgent.GetNavigationMap(), newPosition);
		GD.Print("pos: ", pos);
		
		DebugDraw.Sphere(pos, 1, Colors.Bisque);

		_newTarget = pos;
		blackboard.SetValue("target_movement_position", _newTarget);
		blackboard.SetValue("target_movement_position_reached", false);

	}

}
