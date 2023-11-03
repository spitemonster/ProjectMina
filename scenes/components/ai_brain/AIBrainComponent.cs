using Godot;
using Godot.Collections;


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
	[Export] protected BlackboardComponent Blackboard;
	[Export] protected AttentionComponent AttentionComponent;

	public CharacterBase Pawn { get; private set; }
	public Vector2 MovementDirection { get; protected set; }

	private CombatGridPoint currentGridPoint;
	private bool _avoidanceEnabledDefault = false;

	private BlackboardAsset _blackboardAsset;

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

		if (Blackboard.Blackboard is BlackboardAsset bb)
		{
			_blackboardAsset = bb;
		}

		// Blackboard?.SetValue("max_health", Pawn.CharacterHealth.MaxHealth);

		Pawn.CharacterHealth.HealthChanged += (double newHealth, bool wasDamage) =>
		{
			Blackboard?.SetValue("current_health", newHealth);

			if (wasDamage)
			{
				Blackboard?.SetValue("last_received_damage", Time.GetUnixTimeFromSystem());
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

				if (character is PlayerCharacter p)
				{
					Blackboard.SetValue("enemy_visible", true);
				}
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

				if (character is PlayerCharacter p)
				{
					Blackboard.SetValue("enemy_visible", false);
				}
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

		if (Pawn is AICharacter a)
		{
			Label3D enemySeen = a.GetNode<Label3D>("%enemy_seen_label");

			enemySeen.Text = "Enemy Visible: " + Blackboard.GetValue("enemy_visible");
		}
	}
}
