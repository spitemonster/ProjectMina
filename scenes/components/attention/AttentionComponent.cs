using Godot;

namespace ProjectMina;

[GlobalClass, Icon("res://_dev/icons/icon--magnifier.svg")]
public partial class AttentionComponent : ComponentBase
{
	[Signal] public delegate void FocusChangedEventHandler(Node3D newFocus, Node3D previousFocus);
	[Signal] public delegate void FocusGainedEventHandler(Node3D newFocus);
	[Signal] public delegate void FocusLostEventHandler();

	[Export] public ShapeCast3D FocusCast;
	public Node3D CurrentFocus { get; private set; } = null;
	

	public bool SetFocus(Node3D newFocus)
	{
		// no need to set if they're already the same
		if (CurrentFocus == newFocus)
		{
			return false;
		}

		// if no focus and they're not equal, newFocus isn't null, so emit this
		// this accounts for situations where a player may shift their focus from one item directly to another
		if (CurrentFocus == null)
		{
			EmitSignal(SignalName.FocusGained, newFocus);
		}

		EmitSignal(SignalName.FocusChanged, newFocus, CurrentFocus);
		CurrentFocus = newFocus;
		GD.Print("set focus: ", newFocus);
		return true;
	}

	public bool LoseFocus()
	{
		if (CurrentFocus == null)
		{
			return false;
		}
		
		GD.Print("focus lost");

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
