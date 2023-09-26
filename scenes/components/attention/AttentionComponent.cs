using Godot;

namespace ProjectMina;

[Tool]
[GlobalClass, Icon("res://_dev/icons/icon--magnifier.svg")]
public partial class AttentionComponent : ComponentBase
{
	[Signal] public delegate void FocusChangedEventHandler(Node3D newFocus, Node3D previousFocus);
	[Signal] public delegate void FocusGainedEventHandler(Node3D newFocus);
	[Signal] public delegate void FocusLostEventHandler();

	public Node3D CurrentFocus { get; private set; } = null;

	public bool SetFocus(Node3D newFocus)
	{
		// no need to set if they're already the same
		if (CurrentFocus == newFocus)
		{
			return false;
		}

		// if no focus and they're not equal, newFocus isn't null, so emit this
		if (CurrentFocus == null)
		{
			EmitSignal(SignalName.FocusGained, newFocus);
		}

		EmitSignal(SignalName.FocusChanged, newFocus, CurrentFocus);
		CurrentFocus = newFocus;
		return true;
	}

	public bool LoseFocus()
	{
		if (CurrentFocus == null)
		{
			return false;
		}

		// use SetFocus to trigger changed event handler
		SetFocus(null);
		EmitSignal(SignalName.FocusLost);
		return true;
	}

	public override void _Ready()
	{
		base._Ready();
	}
}
