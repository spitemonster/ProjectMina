using Godot;
using Godot.Collections;

namespace ProjectMina;

// public enum InteractionType : int
// {
// Use,
// Equip,
// Take
// };

public abstract partial class InteractableComponent : ComponentBase
{
	[Signal] public delegate void FocusReceivedEventHandler();
	[Signal] public delegate void FocusLostEventHandler();
	[Signal] public delegate void InteractionStartedEventHandler(CharacterBase character);
	[Signal] public delegate void InteractionCompletedEventHandler();
	[Signal] public delegate void InteractionCanceledEventHandler();
	[Signal] public delegate void InteractionDisabledEventHandler();
	[Signal] public delegate void InteractionEnabledEventHandler();

	public bool CanInteract { get; protected set; } = true;
	private bool _interacting = false;
	private CharacterBase _interactingCharacter;
	
	[Export] public string PromptOverride;
	
	public virtual void ReceiveFocus(CharacterBase character) {}
	public virtual void LoseFocus() {}
	public virtual bool Interact(CharacterBase character)
	{
		if (!CanInteract || _interacting)
		{
			return false;
		}
		
		_interactingCharacter = character;
		CanInteract = false;
		_interacting = true;
		EmitSignal(SignalName.InteractionStarted, character);
		return true;
	}
	public virtual void CompleteInteraction(CharacterBase character)
	{
        _interactingCharacter = null;
		CanInteract = true;
		_interacting = false;
		EmitSignal(SignalName.InteractionCompleted);
	}
	public virtual void CancelInteraction(CharacterBase character)
	{
        _interactingCharacter = null;
		CanInteract = true;
		_interacting = false;
		EmitSignal(SignalName.InteractionCanceled);
	}
	public virtual void EnableInteract()
	{
		CanInteract = true;
		EmitSignal(SignalName.InteractionEnabled);
	}

	public virtual void DisableInteract()
	{
		CanInteract = false;
		EmitSignal(SignalName.InteractionDisabled);
	}
}