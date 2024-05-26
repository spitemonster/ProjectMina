using Godot;
using System.IO;
using System.Threading.Tasks;

namespace ProjectMina.BehaviorTree;

public partial class FollowCurrentTarget : Action
{
	private bool _followed = false;

	protected override async Task<ActionStatus> _Tick(AgentComponent agent, BlackboardComponent blackboard)
	{
		GD.Print("should follow");
		await Task.Run(() =>
		{
			CallDeferred("FollowTarget", agent, blackboard);
		});

		Succeed();
		return Status;
	}

	private void FollowTarget(AgentComponent agent, BlackboardComponent blackboard)
	{
		GD.Print("should follow");
		NodePath currentFocusPath = (NodePath)blackboard.GetValue("current_focus");

		if (currentFocusPath == new NodePath() || currentFocusPath == null)
		{
			_followed = false;
		}

		if (GetNode(currentFocusPath) is Node3D node)
		{
			blackboard.SetValue("target_movement_position", node.GlobalPosition);
		}
		else
		{
			_followed = false;
		}

		_followed = true;
	}
}
