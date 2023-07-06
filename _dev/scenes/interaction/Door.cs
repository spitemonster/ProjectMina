using Godot;

namespace ProjectMina;

public partial class Door : StaticBody3D
{

	[Export]
	protected Vector3 openRotation;
	[Export]
	protected Vector3 closedRotation;
	[Export]
	protected float openSpeed = 10.0f;

	public bool IsOpen { get; }
	private Interaction _interaction;
	private bool _isOpen;

	private Tween _tween;

	public override void _Ready()
	{
		if (GetNode("Interaction") is Interaction i)
		{
			_interaction = i;

			_interaction.Used += ToggleOpen;
		}

		_tween = GetTree().CreateTween();
		_tween.Stop();
	}

	private void ToggleOpen(CharacterBase _interactingCharacter)
	{
		Tween t = GetTree().CreateTween();
		Vector3 targetRotation = _isOpen ? closedRotation : openRotation;
		PropertyTweener tw = t.TweenProperty(this, "rotation_degrees", targetRotation, openSpeed);
		tw.Finished += () => { _isOpen = !_isOpen; };
	}
}
