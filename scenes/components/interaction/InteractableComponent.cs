using Godot;
using Godot.Collections;
namespace ProjectMina;

public partial class InteractableComponent : ComponentBase
{
	[Signal] public delegate void FocusReceivedEventHandler();
	[Signal] public delegate void FocusLostEventHandler();
	[Signal] public delegate void InteractionStartedEventHandler();
	[Signal] public delegate void InteractionEndedEventHandler(bool success);
	[Signal] public delegate void InteractionFinishedEventHandler();
	[Signal] public delegate void InteractionDisabledEventHandler();
	[Signal] public delegate void InteractionEnabledEventHandler();
	public bool CanInteract { get; protected set; }
	
	[ExportCategory("Interaction")]
	[Export] public Control InteractionIndicator;

	[ExportCategory("Animation")]
	[Export] protected AnimationPlayer AnimPlayer;
	[Export] protected AnimationTree AnimTree;
	[Export] protected Array<Animation> FirstPersonInteractionAnimations = new();
	[Export] protected Array<Animation> ThirdPersonInteractionAnimations = new();
	
	private bool _hasIndicator;

	public override void _Ready()
	{
		if (InteractionIndicator == null)
		{
			return;
		}

		_hasIndicator = true;
		InteractionIndicator.Visible = false;
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
		EmitSignal(SignalName.InteractionStarted);
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

	public Animation GetFirstPersonInteractionAnim()
	{
		return FirstPersonInteractionAnimations.PickRandom();
	}

	// public Animation GetThirdPersonInteractionAnim()
	// {
	// 	return ThirdPersonInteractionAnimations.PickRandom();
	// }
}