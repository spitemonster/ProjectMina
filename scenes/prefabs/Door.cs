using Godot;

namespace ProjectMina;

public partial class Door : StaticBody3D
{

	[Export] protected Vector3 OpenRotation;
	[Export] protected Vector3 ClosedRotation;
	[Export] protected float OpenDuration = 2.0f;

	public bool IsOpen { get; private set; }
	private UsableComponent _usableComponent;
	private bool _isOpen;
	private Tween _tween;

	public override void _Ready()
	{
		// redo!
		CallDeferred("_Setup");
	}

	private void _Setup()
	{
		if (GetNode("Usable") is UsableComponent u)
		{
			_usableComponent = u;
			_usableComponent.InteractionStarted += _ToggleOpen;
			GD.Print("door has interactable component");
		}
		else
		{
			GD.PrintErr("door doesn't have usable component");
		}
		
		_tween = GetTree().CreateTween();
		_tween.Stop();
	}

	private void _ToggleOpen(CharacterBase interactingCharacter)
	{
		GD.Print("should open");
		Tween t = GetTree().CreateTween();
		Vector3 targetRotation = IsOpen ? ClosedRotation : OpenRotation;
		PropertyTweener tw = t.TweenProperty(this, "rotation_degrees", targetRotation, OpenDuration);
		tw.Finished += () => { IsOpen = !IsOpen; _usableComponent.CompleteInteraction(interactingCharacter); };
	}
}
