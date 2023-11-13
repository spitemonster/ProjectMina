using Godot;

namespace ProjectMina;

[GlobalClass]
public partial class InteractableComponent : ComponentBase
{
	[Signal] public delegate void FocusReceivedEventHandler();
	[Signal] public delegate void FocusLostEventHandler();
	[Signal] public delegate void InteractionStartedEventHandler();
	[Signal] public delegate void InteractionEndedEventHandler(bool success);
	[Signal] public delegate void InteractionFinishedEventHandler();
	[Export] public Control InteractionIndicator;
	private bool _hasIndicator = false;

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

	public virtual void Interact()
	{
		EmitSignal(SignalName.InteractionStarted);
	}

	public virtual void EndInteract()
	{
		EmitSignal(SignalName.InteractionEnded, true);
	}
}