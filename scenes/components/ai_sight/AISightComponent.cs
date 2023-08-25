using Godot;
using Godot.Collections;
namespace ProjectMina;

[GlobalClass]
public partial class AISightComponent : Node3D
{
	// intended to be used in the array rather than direct references to characters
	class VisibilityContainer
	{
		public CharacterBase Character = new();
		public float Visibility = 1.0f;
	}

	public Array<CharacterBase> CharactersInSightRadius { get; private set; } = new();
	public Array<CharacterBase> VisibleCharacters { get; private set; } = new();

	[Signal]
	public delegate void CharacterEnteredSightRadiusEventHandler(CharacterBase character);
	[Signal]
	public delegate void CharacterExitedSightRadiusEventHandler(CharacterBase character);
	[Signal]
	public delegate void CharacterEnteredLineOfSightEventHandler(CharacterBase character);
	[Signal]
	public delegate void CharacterExitedLineOfSightEventHandler(CharacterBase character);

	[Export]
	protected Area3D _sightCollision;

	private Array<Rid> exclude = new();
	private int _visibilityCheckIndex;

	public override void _Ready()
	{
		if (_sightCollision != null)
		{
			_sightCollision.BodyEntered += CheckShouldObserveBody;
			_sightCollision.BodyExited += CheckRemoveBody;

			CallDeferred("CheckInitialOverlaps");
			exclude.Add(GetOwner<CharacterBase>().GetRid());
		}

		Debug.Assert(_sightCollision != null, "no sight collision");
	}

	public override void _PhysicsProcess(double delta)
	{
		if (CharactersInSightRadius.Count > 0 && CharactersInSightRadius[_visibilityCheckIndex] is CharacterBase visibilityCheckTarget)
		{
			CheckCharacterVisibility(visibilityCheckTarget);
			CalculateCharacterVisibility(visibilityCheckTarget);
		}
		else if (_visibilityCheckIndex > 0)
		{
			_visibilityCheckIndex = 0;
		}
	}

	private float CalculateCharacterVisibility(CharacterBase character)
	{
		Vector3 directionToCharacter = (character.GlobalPosition - GlobalPosition).Normalized();

		float dotProduct = Mathf.Clamp(GlobalTransform.Basis.Z.Dot(directionToCharacter), 0, 1);
		float distanceToCharacter = (character.GlobalPosition - GetOwner<Node3D>().GlobalPosition).Length();
		float clampedDistanceRange = (float)Mathf.Clamp((1 - Mathf.Round(((distanceToCharacter - .5f) / 30.0f) * 100.0) / 100.0), 0, 1);

		float roughVisibility = Mathf.Round((dotProduct * (clampedDistanceRange * 1.25f)) * 100) / 100;
		return roughVisibility;
	}

	private void CheckRemoveBody(Node3D body)
	{
		if (body is CharacterBase c && CharactersInSightRadius.Contains(c))
		{
			EmitSignal(SignalName.CharacterExitedSightRadius, c);
			CharactersInSightRadius.Remove(c);

			if (VisibleCharacters.Contains(c))
			{
				EmitSignal(SignalName.CharacterExitedLineOfSight);
				VisibleCharacters.Remove(c);
			}
		}
	}

	private void CheckInitialOverlaps()
	{
		Array<Node3D> initialOverlaps = _sightCollision.GetOverlappingBodies();

		foreach (Node3D body in initialOverlaps)
		{
			if (body is CharacterBase c)
			{
				CharactersInSightRadius.Add(c);
				EmitSignal(SignalName.CharacterEnteredSightRadius, c);
			}
		}
	}

	private void CheckCharacterVisibility(CharacterBase visibilityCheckTarget)
	{

		// insert some kind of check to see if we even care to watch this character
		PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
		CollisionShape3D targetCharacterBody = visibilityCheckTarget.CharacterBody;

		if (targetCharacterBody == null)
		{

		}

		HitResult traceResult = Trace.Line(spaceState, GlobalPosition, targetCharacterBody.GlobalPosition, exclude);

		if (traceResult != null && traceResult.Collider == visibilityCheckTarget)
		{
			if (!VisibleCharacters.Contains(visibilityCheckTarget))
			{
				VisibleCharacters.Add(visibilityCheckTarget);
				EmitSignal(SignalName.CharacterEnteredLineOfSight, visibilityCheckTarget);
			}
		}
		else
		{
			if (VisibleCharacters.Contains(visibilityCheckTarget))
			{
				VisibleCharacters.Remove(visibilityCheckTarget);
				EmitSignal(SignalName.CharacterExitedLineOfSight, visibilityCheckTarget);
			}
		}

		if (_visibilityCheckIndex == CharactersInSightRadius.Count - 1)
		{
			_visibilityCheckIndex = 0;
		}
		else
		{
			_visibilityCheckIndex += 1;
		}
	}

	private void CheckShouldObserveBody(Node3D body)
	{
		if (body != null && body is CharacterBase c)
		{
			if (!CharactersInSightRadius.Contains(c))
			{
				CharactersInSightRadius.Add(c);
				EmitSignal(SignalName.CharacterEnteredSightRadius, c);
			}
		}
	}
}
