using Godot;
using Godot.Collections;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ProjectMina.BehaviorTree;

public partial class FindRangedWeapon : Action
{
	[Export] public float SearchRadius = 10.0f;

	protected override EActionStatus _Tick(AIControllerComponent controller, BlackboardComponent blackboard)
	{
		FindWeapon(controller, blackboard);
		return Status;
	}

	private void FindWeapon(AIControllerComponent character, BlackboardComponent blackboard)
	{
		
	}
}
