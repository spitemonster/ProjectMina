using Godot;
using Godot.Collections;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ProjectMina.BehaviorTree;

[Tool]
[GlobalClass]
public partial class FindRangedWeapon : Action
{
	[Export] public float SearchRadius = 10.0f;
	[Signal] public delegate void SearchCompletedEventHandler(bool weaponFound);

	protected override async Task<ActionStatus> _Tick(AICharacter character, BlackboardComponent blackboard)
	{
		FindWeapon(character, blackboard);
		await ToSignal(this, SignalName.SearchCompleted);
		return Status;
	}

	private void FindWeapon(AICharacter character, BlackboardComponent blackboard)
	{
		Vector3 characterPosition = character.GlobalPosition;

		Array<Equipment> weaponsNearby = character.searchComponent.GetWeaponsInRadius();

		if (weaponsNearby.Count < 1)
		{
			Fail();
			EmitSignal(SignalName.SearchCompleted, false);
			return;
		}

		Equipment w = weaponsNearby[0];
		Node3D p = (Node3D)w.GetParent();
		float dist = (p.GlobalPosition - character.GlobalPosition).Length();

		if (dist <= 1.0)
		{
			Succeed();
			EmitSignal(SignalName.SearchCompleted);
		}
		else
		{
			blackboard.SetValue("target_movement_position", p.GlobalPosition);

			character.Brain.NavigationAgent.TargetReached += () =>
			{
				GD.Print("character can equip weapon now");
				Succeed();
				EmitSignal(SignalName.SearchCompleted);
			};
		}
	}
}
