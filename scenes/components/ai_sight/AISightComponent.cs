using Godot;
using Godot.Collections;
namespace ProjectMina;

[Tool]
[GlobalClass, Icon("res://_dev/icons/icon--eye.svg")]
public partial class AISightComponent : ComponentBase
{
	// intended to be used in the array rather than direct references to characters
	class VisibilityContainer
	{
		public CharacterBase Character = new();
		public float Visibility = 1.0f;
	}

	public Array<CharacterBase> CharactersInSightRadius { get; private set; } = new();
	public Array<CharacterBase> SeenCharacters { get; private set; } = new();

	/// <summary>
	/// fired when character enters sight radius, regardless of line of sight. each character could be seen.
	/// </summary>
	/// <param name="character">
	/// new character
	/// </param>
	[Signal] public delegate void CharacterEnteredSightRadiusEventHandler(CharacterBase character);
	/// <summary>
	/// fired when character leaves sight radius, regardless of line of sight. character can no longer be seen.
	/// </summary>
	/// <param name="character">
	/// left character
	/// </param>
	[Signal] public delegate void CharacterExitedSightRadiusEventHandler(CharacterBase character);
	/// <summary>
	/// fired when character is added to the seencharacters array; there is a clear line of sight to this character.
	/// </summary>
	/// <param name="character">
	/// seen character
	/// </param>
	[Signal] public delegate void CharacterEnteredLineOfSightEventHandler(CharacterBase character);
	/// <summary>
	/// fired when character to which we previously had line of sight is now obscured
	/// </summary>
	/// <param name="exclude">
	/// unseen character
	/// </param>
	[Signal] public delegate void CharacterExitedLineOfSightEventHandler(CharacterBase character);
	[Export] protected Area3D _sightCollision;

	private Array<Rid> exclude = new();
	private int _visibilityCheckIndex;
	private AICharacter _owner;

	public override void _Ready()
	{
		base._Ready();

		if (Engine.IsEditorHint() || !_active)
		{
			return;
		}

		if (_debug)
		{
			System.Diagnostics.Debug.Assert(_sightCollision != null, "no sight collision");
		}

		_owner = GetOwner<AICharacter>();
		_sightCollision.BodyEntered += CheckShouldObserveBody;
		_sightCollision.BodyExited += CheckRemoveBody;

		if (!Engine.IsEditorHint())
		{
			exclude.Add(GetOwner<CharacterBase>().GetRid());
		}

		CallDeferred("CheckInitialOverlaps");
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		foreach (CharacterBase c in CharactersInSightRadius)
		{
			if (HaveLineOfSightToCharacter(c))
			{
				TryAddSeenCharacter(c);
			}
			else
			{
				TryRemoveSeenCharacter(c);
			}
		}
	}

	private float CalculateCharacterVisibility(CharacterBase character)
	{

		return 1f;
	}

	private void CheckRemoveBody(Node3D body)
	{
		if (body is CharacterBase c)
		{
			TryRemoveCharacterFromSightRadius(c);
			TryRemoveSeenCharacter(c);
		}
	}

	private void CheckInitialOverlaps()
	{
		Array<Node3D> initialOverlaps = _sightCollision.GetOverlappingBodies();

		foreach (Node3D body in initialOverlaps)
		{
			if (body is CharacterBase c && c != _owner)
			{
				TryAddCharacterToSightRadius(c);
			}
		}
	}

	/// <summary>
	/// line trace from owner's eyes to target character's eyes or chest based on alertedness
	/// </summary>
	/// <param name="targetCharacter">character for which we are testing line of sight</param>
	/// <returns>true if owner has line of sight, else false</returns>
	private bool HaveLineOfSightToCharacter(CharacterBase targetCharacter)
	{
		PhysicsDirectSpaceState3D spaceState = _owner.GetWorld3D().DirectSpaceState;
		CollisionShape3D targetCharacterBody = targetCharacter.CharacterBody;
		Vector3 traceOrigin = _owner.Eyes.GlobalPosition;
		Vector3 traceEnd = targetCharacter.Chest.GlobalPosition;
		HitResult traceResult = Trace.Line(spaceState, traceOrigin, targetCharacterBody.GlobalPosition, exclude);

		if (_debug)
		{
			DebugDraw.Line(traceOrigin, traceEnd, Colors.Red);
		}

		if (traceResult == null || !traceResult.Collider.Equals(targetCharacter))
		{
			return false;
		}

		return true;
	}

	private void CheckShouldObserveBody(Node3D body)
	{
		if (body != null && body is CharacterBase c && c != _owner)
		{
			TryAddCharacterToSightRadius(c);
		}
	}

	private bool TryAddCharacterToSightRadius(CharacterBase character)
	{
		if (CharactersInSightRadius.Contains(character))
		{
			return false;
		}

		CharactersInSightRadius.Add(character);
		EmitSignal(SignalName.CharacterEnteredSightRadius, character);

		if (_debug)
		{
			GD.Print(character.Name + " entered sight radius of " + GetOwner<Node>().Name);
			DebugDraw.Sphere(character.GlobalTransform, 1, Colors.Red, 3f);
		}
		return true;
	}

	private bool TryRemoveCharacterFromSightRadius(CharacterBase character)
	{

		if (!CharactersInSightRadius.Contains(character))
		{
			return false;
		}

		CharactersInSightRadius.Remove(character);
		EmitSignal(SignalName.CharacterExitedSightRadius, character);

		if (_debug)
		{
			GD.Print(character.Name + " exited sight radius of " + GetOwner<Node>().Name);
			DebugDraw.Sphere(character.GlobalTransform, 1, Colors.Red, 3f);
		}

		return true;
	}

	private bool TryAddSeenCharacter(CharacterBase character)
	{

		if (SeenCharacters.Contains(character))
		{
			return false;
		}

		SeenCharacters.Add(character);
		EmitSignal(SignalName.CharacterEnteredLineOfSight, character);

		if (_debug)
		{
			GD.Print(character.Name + " entered line of sight of " + GetOwner<Node>().Name);
		}
		return true;
	}

	private bool TryRemoveSeenCharacter(CharacterBase character)
	{
		if (!SeenCharacters.Contains(character))
		{
			return false;
		}

		SeenCharacters.Remove(character);
		EmitSignal(SignalName.CharacterExitedLineOfSight, character);

		if (_debug)
		{
			GD.Print(character.Name + " exited line of sight of " + GetOwner<Node>().Name);
		}
		return true;
	}
}
