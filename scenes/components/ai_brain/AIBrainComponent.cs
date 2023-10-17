using System;
using Godot;
using ProjectMina.BehaviorTree;

namespace ProjectMina;
/// <summary>
/// 	Functions as the AI stand in for a player. Controls a CharacterBase Pawn in the world, makes decisions based on its knowledge and sensory data and directs its pawn to action.
/// </summary>

[Tool]
[GlobalClass, Icon("res://_dev/icons/icon--brain.svg")]
public partial class AIBrainComponent : ControllerComponent
{
	[Export] public NavigationAgent3D NavigationAgent { get; protected set; }
	[Export] protected AISightComponent SightComponent;
	[Export] protected AIHearingComponent HearingComponent;
	[Export] protected BehaviorTreeComponent BehaviorTree;
	[Export] protected BlackboardComponent Blackboard;
	[Export] protected AttentionComponent AttentionComponent;

	public CharacterBase Pawn { get; private set; }
	public Vector2 MovementDirection { get; protected set; }

	private CombatGridPoint currentGridPoint;
	private bool _avoidanceEnabledDefault = false;

	public Node3D GetCurrentFocus()
	{
		return AttentionComponent.CurrentFocus;
	}

	public Vector3 GetTargetPosition()
	{
		return (Vector3)Blackboard.GetValue("target_movement_position");
	}

	public override void _Ready()
	{
		base._Ready();
		// all these components are required for the ai brain to function, so pitch a fit right a way
		if (_debug)
		{
			System.Diagnostics.Debug.Assert(AttentionComponent != null, "no attention component");
			System.Diagnostics.Debug.Assert(SightComponent != null, "no ai sight component");
			System.Diagnostics.Debug.Assert(HearingComponent != null, "no ai hearing component");
			System.Diagnostics.Debug.Assert(NavigationAgent != null, "no navigation component");
			System.Diagnostics.Debug.Assert(BehaviorTree != null, "no behavior tree component");
			System.Diagnostics.Debug.Assert(Blackboard != null, "no blackboard component");
		}

		// none of this needs to run in the editor and it DEFINITELY does not need to tick in the editor
		if (Engine.IsEditorHint())
		{
			SetProcess(false);
			SetPhysicsProcess(false);
			return;
		}

		Pawn = GetOwner<CharacterBase>();

		if (_debug)
		{
			System.Diagnostics.Debug.Assert(Pawn != null, "No controlled character");
		}

		// Blackboard?.SetValue("max_health", Pawn.CharacterHealth.MaxHealth);

		Pawn.CharacterHealth.HealthChanged += (double newHealth, bool wasDamage) =>
		{
			Blackboard?.SetValue("current_health", newHealth);

			if (wasDamage)
			{
				Blackboard?.SetValue("last_received_damage", DateTime.Now.ToBinary());
			}
		};

		SightComponent.CharacterEnteredSightRadius += (character) =>
		{
			// if (character is PlayerCharacter p)
			// {
			// 	SeeEnemy(p);
			// }
		};

		SightComponent.CharacterEnteredLineOfSight += (character) =>
		{
			if (GetCurrentFocus() == null)
			{
				AttentionComponent?.SetFocus(character);
			}
		};

		SightComponent.CharacterExitedSightRadius += (character) =>
		{

		};

		SightComponent.CharacterExitedLineOfSight += (character) =>
		{
			if (character.Equals(GetCurrentFocus()))
			{
				AttentionComponent?.LoseFocus();
			}
		};

		Pawn.CharacterMovement.MovementStateChanged += (MovementComponent.MovementState newState) =>
		{
			Blackboard?.SetValue("movement_state", (int)newState);
		};

		NavigationAgent.TargetReached += () =>
		{
			GD.Print("position_reached");
			Blackboard?.SetValue("target_movement_position_reached", true);
		};

		AttentionComponent.FocusChanged += (Node3D newFocus, Node3D previousFocus) =>
		{
			bool addToBlackboard = Blackboard?.SetValue("current_focus", newFocus?.GetPath()) ?? false;

			if (addToBlackboard)
			{
				GD.Print("successfully changed focus: ", newFocus?.Name);
			}

			if (newFocus == null)
			{
				GD.Print("lost focus!");
			}
		};

	}

	public override void _Process(double delta)
	{
		base._Process(delta);
	}
}
