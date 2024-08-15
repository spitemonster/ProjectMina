using System;
using System.Diagnostics;
using GDebugPanelGodot.Extensions;
using Godot;
using ProjectMina.BehaviorTree;
namespace ProjectMina;

[GlobalClass]
public partial class AICharacter : CharacterBase
{
	[Export] public float TurnRate { get; protected set; } = 5f;
	[Export] public SteeringComponent Steering { get; protected set; }
	[Export] public AIPerceptionComponent CharacterPerception { get; protected set; }

	private CharacterMovementStateMachine _msm;
	
	[Export] protected ProgressBar NoticeBar;
	[Export] protected ProgressBar AlertBar;

	public AIControllerComponent AIController { get; protected set; }
	public Label3D AwarenessLabel { get; protected set; }
	public Label3D DetectionLabel { get; protected set; }

	protected AnimationNodeStateMachinePlayback AnimStateMachine;

	protected Node3D RightHandSlot;
	protected Node3D LeftHandSlot;
	protected Node3D LeftHipSlot;

	[Export] private RigidBody3D _testWeapon;

	private DevMonitor _lookAngleMonitor;

	public float LookRotationRad { get; protected set; } = 0;
	public float LookRotationDeg { get; protected set; } = 0;
	public float PreviousLookRotation { get; protected set; }
	public float PreviousVelocity { get; protected set; }

	[Export] protected SkeletonIK3D LookIK;

	public bool DrawWeapon()
	{
		AnimTree.Set("parameters/draw_weapon/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
		return true;
	}

	public bool SheathWeapon()
	{
		AnimTree.Set("parameters/sheath_weapon/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
		return true;
	}

	public bool EquipWeapon()
	{
		_testWeapon.GetParent().RemoveChild(_testWeapon);
		RightHandSlot.AddChild(_testWeapon);
		_testWeapon.GlobalTransform = RightHandSlot.GlobalTransform;
		return true;
	}

	public bool UnequipWeapon()
	{
		_testWeapon.GetParent().RemoveChild(_testWeapon);
		LeftHipSlot.AddChild(_testWeapon);
		_testWeapon.GlobalTransform = LeftHipSlot.GlobalTransform;
		return true;
	}

	public override void _Ready()
	{
		base._Ready();

		Controller = GetNodeOrNull<ControllerComponent>("%Controller");

		AIController = (AIControllerComponent)Controller;

		if (AIController != null)
		{
			AIController.AgentStateChanged += _OnAgentStateChanged;
		}
		
		_msm = GetNodeOrNull<CharacterMovementStateMachine>("%MSM");
		
		AnimStateMachine = (AnimationNodeStateMachinePlayback)AnimTree?.Get("parameters/character_state/playback");
		
		AwarenessLabel = GetNodeOrNull<Label3D>("%AwarenessLabel");
		DetectionLabel = GetNodeOrNull<Label3D>("%DetectionLabel");
		
		RightHandSlot = GetNodeOrNull<Node3D>("%RightHandSlot");
		LeftHandSlot = GetNodeOrNull<Node3D>("%LeftHandSlot");
		LeftHipSlot = GetNodeOrNull<Node3D>("%LeftHipSlot");
		
		Global.Data.AddAICharacter(this);

		CallDeferred("_InitDev");
	}

	private void _InitDev()
	{
		_lookAngleMonitor = Dev.UI.AddDevMonitor("ai look angle", Colors.Goldenrod, "AI");		
	}

	public override void _PhysicsProcess(double delta)
	{
		if (AIController == null)
		{
			AIController = (AIControllerComponent)Controller;

			if (AIController == null)
			{
				return;
			}
		}

		if (ControlInput.Length() > 0)
		{
			GlobalBasis =
				GlobalTransform.InterpolateWith(
					GlobalTransform.LookingAt(GlobalPosition + new Vector3(ControlInput.X, 0, ControlInput.Y), Vector3.Up), (float)delta * TurnRate).Basis;	
		}

		var traceOrigin = GlobalPosition + new Vector3(0, .5f, 0);
		
		DebugDraw.Line(traceOrigin, traceOrigin + new Vector3(MovementInput.Normalized().X, 0, MovementInput.Normalized().Y), Colors.Red);
		DebugDraw.Line(traceOrigin, traceOrigin + new Vector3(ControlInput.Normalized().X, 0, ControlInput.Normalized().Y), Colors.Green);

		LookRotationRad = MovementInput.Normalized().AngleTo(ControlInput.Normalized());
		LookRotationDeg = Mathf.RadToDeg(LookRotationRad);
		
		_lookAngleMonitor?.SetValue( LookRotationDeg.ToString() );
		
		
		var localVelocity = _msm.GetCharacterVelocity(MovementInput, delta);

		if (Steering != null)
		{
			localVelocity = Steering.CalculateSteeringVelocity(localVelocity);
		}

		if (CanMove)
		{
			Velocity = localVelocity;	
		}
		 
		base._PhysicsProcess(delta);
		
		PreviousLookRotation = LookRotationDeg;
		PreviousVelocity = Velocity.Length();
	}

	private void _OnAgentStateChanged(EAgentState agentState)
	{
		switch (agentState)
		{
			case EAgentState.Idle:
				AnimStateMachine.Travel("idle");
				break;
			case EAgentState.Combat:
				AnimStateMachine.Travel("combat");
				break;
			default:
				AnimStateMachine.Travel("idle");
				break;
		}
	} 
}
