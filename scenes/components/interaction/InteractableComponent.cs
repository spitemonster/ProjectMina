using Godot;
using Godot.Collections;
using ProjectMina.Goap;

namespace ProjectMina;

public partial class InteractableComponent : ComponentBase
{
	[Signal] public delegate void FocusReceivedEventHandler();
	[Signal] public delegate void FocusLostEventHandler();
	[Signal] public delegate void InteractionStartedEventHandler(CharacterBase character);
	[Signal] public delegate void InteractionEndedEventHandler(bool success);
	[Signal] public delegate void InteractionFinishedEventHandler();
	[Signal] public delegate void InteractionDisabledEventHandler();
	[Signal] public delegate void InteractionEnabledEventHandler();
	public bool CanInteract { get; protected set; }

	// first foray into integrating the interaction system with the goap system
	// enables a goap agent to check outcome of using the smart object
	// and what they can do with it
	[Export] public Array<GoapActionBase> Actions = new();
	[Export] public Dictionary<StringName, Variant> UseState { get; protected set; } = new();

	[ExportCategory("Interaction")]
	[Export] public Control InteractionIndicator;

	[ExportCategory("Animation")]
	[Export] public AnimationPlayer AnimPlayer { get; protected set; }
	[Export] public AnimationTree AnimTree { get; protected set; }
	[Export] public AnimationLibrary FirstPersonInteractionAnimations { get; protected set; }
	
	[Export] public StringName ThirdPersonInteractionAnimName;
	
	// position to which an AI character were to move if they were to attempt to interact
	[Export] public Marker3D ActionPosition { get; protected set; }
	
	private bool _hasIndicator;

	public override void _Ready()
	{
		if (InteractionIndicator != null)
		{
			_hasIndicator = true;
			InteractionIndicator.Visible = false;
		}
	}

	public virtual void ReceiveFocus()
	{
		EmitSignal(SignalName.FocusReceived);

		if (_hasIndicator)
		{
			InteractionIndicator.Visible = true;
		}
	}

	public virtual void LoseFocus()
	{
		EmitSignal(SignalName.FocusLost);

		if (_hasIndicator)
		{
			InteractionIndicator.Visible = false;
		}
	}

	public virtual void Interact(CharacterBase character)
	{
		EmitSignal(SignalName.InteractionStarted, character);
	}

	public virtual void EndInteract()
	{
		EmitSignal(SignalName.InteractionEnded, true);
	}

	public void EnableInteract()
	{
		CanInteract = true;
		EmitSignal(SignalName.InteractionEnabled);
	}

	public void DisableInteract()
	{
		CanInteract = false;
		EmitSignal(SignalName.InteractionDisabled);
	}

	// public Animation GetThirdPersonInteractionAnim()
	// {
	// 	return ThirdPersonInteractionAnimations.PickRandom();
	// }
}