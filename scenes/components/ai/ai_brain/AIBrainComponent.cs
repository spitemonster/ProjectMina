using Godot;
using Godot.Collections;


namespace ProjectMina;

public enum AIAwarenessState : uint
{
	Dead,
	Idle,
	Suspicious,
	Alerted
}

/// <summary>
/// 	Functions as the AI stand in for a player. Controls a CharacterBase Pawn in the world, makes decisions based on its knowledge and sensory data and directs its pawn to action.
/// </summary>

public partial class AIBrainComponent : ControllerComponent
{
	public AICharacter Pawn { get; private set; }
	[Export] public BlackboardComponent Blackboard { get; protected set; }
	[Export] public AIPerceptionComponent Perception { get; protected set; }

	public AIAwarenessState AwarenessLevel = AIAwarenessState.Idle;

	private BlackboardAsset _blackboardAsset;

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
		
		// none of this needs to run in the editor and it DEFINITELY does not need to tick in the editor
		if (Engine.IsEditorHint())
		{
			SetProcess(false);
			SetPhysicsProcess(false);
			return;
		}
		
		Pawn = GetOwner<AICharacter>();
		
		if (EnableDebug)
		{
			System.Diagnostics.Debug.Assert(Blackboard != null, "AI Brain Component does not have Blackboard");
			System.Diagnostics.Debug.Assert(Pawn != null, "AI Brain Component not attached to AI Character.");
		}
		
		if (!Active || Blackboard == null || Pawn == null)
		{
			return;
		}
		
		base._Ready();
		

		if (Blackboard.Blackboard is BlackboardAsset bb)
		{
			_blackboardAsset = bb;
		}
		
		GD.Print("[color=pink]should set max health[/color]");
		GD.Print("[color=pink]", Pawn.CharacterHealth.MaxHealth, "[/color]");

		Blackboard.SetValue("max_health", Pawn.CharacterHealth.MaxHealth);

		Pawn.CharacterHealth.HealthChanged += (double newHealth, bool wasDamage) =>
		{
			Blackboard?.SetValue("current_health", newHealth);

			if (wasDamage)
			{
				Blackboard?.SetValue("last_received_damage", Time.GetUnixTimeFromSystem());
			}
		};

		// Perception.VisibleCharactersUpdated += characters =>
		// {
		// 	Blackboard.SetValue("visible_characters", characters);
		// };

		Perception.CharacterEnteredPerceptionRadius += (CharacterBase character) =>
		{
			// switch (character.Faction)
			// {
			// 	case CharacterFaction.Thief:
			// 		GD.Print("[color=red]AI Brain component added seen character: ", character.Name, "[/color]");
			// 		break;
			// }
		};
		
		Perception.CharacterExitedPerceptionRadius += (CharacterBase character) =>
		{
			// switch (character.Faction)
			// {
			// 	case CharacterFaction.Thief:
			// 		GD.Print("[color=red]AI Brain component added seen character: ", character.Name, "[/color]");
			// 		Blackboard.SetValue("enemy_visible", true);
			// 		Blackboard.SetValue("target_enemy", character);
			// 		break;
			// }
		};

		Pawn.CharacterMovement.MovementStateChanged += (MovementComponent.MovementState newState) =>
		{
			Blackboard?.SetValue("movement_state", (int)newState);
		};

		Pawn.NavigationAgent.TargetReached += () =>
		{
			
		};

		Pawn.CharacterAttention.FocusChanged += (Node3D newFocus, Node3D previousFocus) =>
		{
			// bool addToBlackboard = Blackboard?.SetValue("current_focus", newFocus?.GetPath()) ?? false;
			//
			// if (addToBlackboard)
			// {
			// 	GD.Print("successfully changed focus: ", newFocus?.Name);
			// }
			//
			// if (newFocus == null)
			// {
			// 	GD.Print("lost focus!");
			// }
		};
	}
}
