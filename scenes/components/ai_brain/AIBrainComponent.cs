using System;
using Godot;
using ProjectMina.BehaviorTree;
namespace ProjectMina;

/// <summary>
/// 	Functions as the AI stand in for a player. Controls a CharacterBase Pawn in the world, makes decisions based on its knowledge and sensory data and directs its pawn to action.
/// </summary>
[GlobalClass]
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

	[Export] private bool _debug = false;
	private bool _avoidanceEnabledDefault = false;

	public Node3D GetCurrentFocus()
	{
		return AttentionComponent.CurrentFocus;
	}

	public Vector3 GetTargetPosition()
	{
		return (Vector3)Blackboard.GetValue("target_position");
	}

	public override void _Ready()
	{
		Pawn = GetOwner<CharacterBase>();

		if (_debug)
		{
			System.Diagnostics.Debug.Assert(AttentionComponent != null, "no attention component");
			System.Diagnostics.Debug.Assert(SightComponent != null, "no ai sight component");
			System.Diagnostics.Debug.Assert(HearingComponent != null, "no ai hearing component");
			System.Diagnostics.Debug.Assert(NavigationAgent != null, "no navigation component");
			System.Diagnostics.Debug.Assert(BehaviorTree != null, "no behavior tree component");
			System.Diagnostics.Debug.Assert(Blackboard != null, "no blackboard component");
			System.Diagnostics.Debug.Assert(Pawn != null, "No controlled character");
		}

		Blackboard.SetValue("max_health", Pawn.CharacterHealth.MaxHealth);

		Pawn.CharacterHealth.HealthChanged += (double newHealth, bool wasDamage) =>
		{
			Blackboard.SetValue("current_health", newHealth);

			if (wasDamage)
			{
				Blackboard.SetValue("last_received_damage", DateTime.Now.ToBinary());
			}
		};

		SightComponent.CharacterEnteredSightRadius += (character) =>
		{
			if (character is PlayerCharacter p)
			{
				// SeeEnemy(p);
			}
		};

		SightComponent.CharacterEnteredLineOfSight += (character) =>
		{
			if (GetCurrentFocus() == null)
			{
				AttentionComponent.SetFocus(character);
			}
		};

		SightComponent.CharacterExitedSightRadius += (character) =>
		{

		};

		SightComponent.CharacterExitedLineOfSight += (character) =>
		{
			if (character.Equals(GetCurrentFocus()))
			{
				AttentionComponent.LoseFocus();
			}
		};

		Pawn.CharacterMovement.MovementStateChanged += (MovementComponent.MovementState newState) =>
		{
			Blackboard.SetValue("movement_state", (int)newState);
		};

		NavigationAgent.TargetReached += () =>
		{
			Blackboard.SetValue("target_position_reached", true);
		};

		AttentionComponent.FocusChanged += (Node3D newFocus, Node3D previousFocus) =>
		{
			if (Blackboard.SetValue("current_focus", newFocus?.GetPath()))
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
		Godot.Collections.Array<NodePath> visibleCharacters = new();
		foreach (Node3D n in SightComponent.CharactersInSightRadius)
		{
			visibleCharacters.Add(n.GetPath());
		}

		Blackboard.SetValue("visible_characters", visibleCharacters);

		Godot.Collections.Array<NodePath> seenCharacters = new();

		foreach (Node3D n in SightComponent.SeenCharacters)
		{
			seenCharacters.Add(n.GetPath());
		}

		Blackboard.SetValue("seen_characters", seenCharacters);

		if (GetCurrentFocus() != null)
		{
			Blackboard.SetValue("target_position", GetCurrentFocus().GlobalPosition);
			NavigationAgent.TargetPosition = GetCurrentFocus().GlobalPosition;
		}
	}

	private void SeeEnemy(CharacterBase targetCharacter)
	{
		// Pawn.SetFocus(targetCharacter);
	}
}
