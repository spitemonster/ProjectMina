using Godot;
using System;
using Godot.Collections;
using ProjectMina;

public partial class UtilityAIManager : Node
{
	[Export] public AIPerceptionComponent Perception;

	[Export] public Node BehaviorTreeRoot;
	[Export] public Node HasTargetEnemySensor;
	[Export] public Node DistanceToTargetSensor;

	private Node3D _currentTarget;

	private AICharacter _owner;
	
	public override void _Ready()
	{
		_owner = GetOwner<AICharacter>();
		
		// Perception.CharacterEnteredLineOfSight += character =>
		// {
		// 	if (_currentTarget == null)
		// 	{
		// 		
		// 		_currentTarget = character;
		// 	}
		// };
		//
		// Perception.CharacterExitedLineOfSight += character =>
		// {
		// 	if (_currentTarget == character)
		// 	{
		// 		_currentTarget = null;
		// 	}
		// };
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		var test = new Dictionary();
		test.Add("test-one", "test!");
		test.Add("test-two", "test again!");

		if (BehaviorTreeRoot != null)
		{
			BehaviorTreeRoot.Call("tick", test, delta);
		}
		
		HasTargetEnemySensor.Set("boolean_value", _currentTarget != null);
		DistanceToTargetSensor.Set("from_vector", _owner.GlobalPosition);

		if (_currentTarget != null)
		{
			DistanceToTargetSensor.Set("to_vector", _currentTarget.GlobalPosition);
		}
		else
		{
			DistanceToTargetSensor.Set("to_vector", _owner.GlobalPosition);
		}
	}
}
