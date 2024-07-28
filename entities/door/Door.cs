using Godot;

namespace ProjectMina;

public partial class Door : StaticBody3D
{
	[ExportCategory("Animation")]
	[Export] protected Vector3 OpenRotation;
	[Export] protected Vector3 ClosedRotation;
	[Export] protected float OpenDuration = 2.0f;

	public bool IsOpen { get; private set; }
	private LockableComponent _lockableComponent;
	private UsableComponent _usableComponent;
	private bool _isOpen;
	private Tween _tween;

	private Vector3 _closedRotation;
	private Vector3 _openRotation;

	public override void _Ready()
	{
		_closedRotation = GlobalRotation;
		_openRotation = GlobalRotation + OpenRotation;
		// redo!
		if (GetNode("Usable") is UsableComponent u)
		{
			_usableComponent = u;
			_usableComponent.InteractionStarted += _ToggleOpen;
		}

		if (GetNode("Lockable") is LockableComponent l)
		{
			_lockableComponent = l;
		}
		
		_tween = GetTree().CreateTween();
		_tween.Stop();
	}

	private void _ToggleOpen(CharacterBase interactingCharacter)
	{
		if (_lockableComponent.Locked)
		{
			return;
		}
		
		// TODO: figure out why doors don't rotate relative to their starting rotation
		Tween t = GetTree().CreateTween();
		Vector3 targetRotation = IsOpen ? ClosedRotation : OpenRotation;
		PropertyTweener tw = t.TweenProperty(this, "rotation_degrees", targetRotation, OpenDuration);
		tw.Finished += () => { IsOpen = !IsOpen; _usableComponent.CompleteInteraction(interactingCharacter); };
	}
}
