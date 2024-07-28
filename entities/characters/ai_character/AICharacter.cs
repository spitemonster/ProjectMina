using System;
using System.Diagnostics;
using Godot;
using ProjectMina.BehaviorTree;
namespace ProjectMina;

[GlobalClass]
public partial class AICharacter : CharacterBase
{
	[Export] public SteeringComponent Steering { get; protected set; }
	[Export] public AIPerceptionComponent CharacterPerception { get; protected set; }

	private CharacterMovementStateMachine _msm;
	
	[Export] protected ProgressBar NoticeBar;
	[Export] protected ProgressBar AlertBar;

	private AIControllerComponent _aiController;

	public override void _Ready()
	{
		base._Ready();

		Controller = GetNode<ControllerComponent>("%Controller");

		_msm = GetNode<CharacterMovementStateMachine>("%MSM");
		_aiController = (AIControllerComponent)Controller;
		Global.Data.AddAICharacter(this);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_aiController == null)
		{
			_aiController = (AIControllerComponent)Controller;
		}

		if (ControlInput.Length() > 0)
		{
			GlobalBasis =
				GlobalTransform.InterpolateWith(
					GlobalTransform.LookingAt(GlobalPosition + new Vector3(ControlInput.X, 0, ControlInput.Y), Vector3.Up), (float)delta * 3f).Basis;	
		}
		
		var localVelocity = _msm.GetCharacterVelocity(MovementInput, delta);

		if (Steering != null)
		{
			localVelocity = Steering.CalculateSteeringVelocity(localVelocity);
		}

		Velocity = localVelocity; 
		base._PhysicsProcess(delta);
	}
}
