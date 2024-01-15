using Godot;
using Godot.Collections;


namespace ProjectMina;
/// <summary>
/// 	Functions as the AI stand in for a player. Controls a CharacterBase Pawn in the world, makes decisions based on its knowledge and sensory data and directs its pawn to action.
/// </summary>

public partial class AIBrainComponent : ControllerComponent
{
	// [Export] public NavigationAgent3D NavigationAgent { get; protected set; }
	// [Export] protected AISightComponent SightComponent;
	// [Export] protected AIHearingComponent HearingComponent;
	[Export] public BlackboardComponent Blackboard { get; protected set; }

	public AICharacter Pawn { get; private set; }
	public Vector2 MovementDirection { get; protected set; }

	private CombatGridPoint currentGridPoint;
	private bool _avoidanceEnabledDefault = false;

	private BlackboardAsset _blackboardAsset;

	private Node3D _currentTarget = null;

	public Node3D GetCurrentFocus()
	{
		return Pawn.CharacterAttention.CurrentFocus;
	}

	public Vector3 GetTargetPosition()
	{
		return (Vector3)Blackboard.GetValue("target_movement_position");
	}

	public override void _Ready()
	{
		if (!Active)
		{
			return;
		}
		
		if (EnableDebug)
		{
			System.Diagnostics.Debug.Assert(Blackboard != null, "no blackboard component");
		}

		// none of this needs to run in the editor and it DEFINITELY does not need to tick in the editor
		if (Engine.IsEditorHint())
		{
			SetProcess(false);
			SetPhysicsProcess(false);
			return;
		}
		
		if (Blackboard == null)
		{
			return;
		}
		
		base._Ready();

		Pawn = GetOwner<AICharacter>();

		if (EnableDebug)
		{
			System.Diagnostics.Debug.Assert(Pawn != null, "No Pawn");
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

		// SightComponent.CharacterEnteredSightRadius += (character) =>
		// {
		// 	// if (character is PlayerCharacter p)
		// 	// {
		// 	// 	SeeEnemy(p);
		// 	// }
		// };
		//
		// SightComponent.CharacterEnteredLineOfSight += (character) =>
		// {
		// 	// if (GetCurrentFocus() == null)
		// 	// {
		// 	// 	AttentionComponent?.SetFocus(character);
		// 	//
		// 	// 	if (character is PlayerCharacter p)
		// 	// 	{
		// 	// 		Blackboard.SetValue("enemy_visible", true);
		// 	// 	}
		// 	// }
		// };
		//
		// SightComponent.CharacterExitedSightRadius += (character) =>
		// {
		//
		// };
		//
		// SightComponent.CharacterExitedLineOfSight += (character) =>
		// {
		// 	// if (character.Equals(GetCurrentFocus()))
		// 	// {
		// 	// 	AttentionComponent?.LoseFocus();
		// 	//
		// 	// 	if (character is PlayerCharacter p)
		// 	// 	{
		// 	// 		Blackboard.SetValue("enemy_visible", false);
		// 	// 	}
		// 	// }
		// };

		Pawn.CharacterMovement.MovementStateChanged += (MovementComponent.MovementState newState) =>
		{
			Blackboard?.SetValue("movement_state", (int)newState);
		};

		Pawn.NavigationAgent.TargetReached += () =>
		{
			
		};

		Pawn.CharacterAttention.FocusChanged += (Node3D newFocus, Node3D previousFocus) =>
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

		if (Pawn.CharacterHealth != null)
		{
			// Blackboard?.SetValue("current_health", Pawn.CharacterHealth.CurrentHealth);
			Blackboard?.SetValue("max_health", Pawn.CharacterHealth.MaxHealth);
		}
	}
}
