using Godot;
using Godot.Collections;
namespace ProjectMina;

[Tool]
[GlobalClass, Icon("res://_dev/icons/icon--eye.svg")]
public partial class AISightComponent : ComponentBase
{
	// intended to be used in the array rather than direct references to characters
	public class VisibilityContainer
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
	/// <param name="character">
	/// unseen character
	/// </param>
	[Signal] public delegate void CharacterExitedLineOfSightEventHandler(CharacterBase character);
	[Export] protected Area3D SightCollision;

	private Array<Rid> _exclude = new();
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
			System.Diagnostics.Debug.Assert(SightCollision != null, "no sight collision");
		}

		_owner = GetOwner<AICharacter>();
		SightCollision.BodyEntered += CheckShouldObserveBody;
		SightCollision.BodyExited += CheckRemoveBody;

		if (!Engine.IsEditorHint())
		{
			_exclude.Add(GetOwner<CharacterBase>().GetRid());
		}

		CallDeferred("CheckInitialOverlaps");
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		foreach (var c in CharactersInSightRadius)
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
		if (body is not CharacterBase c) return;
		
		TryRemoveCharacterFromSightRadius(c);
		TryRemoveSeenCharacter(c);
	}

	private void CheckInitialOverlaps()
	{
		var initialOverlaps = SightCollision.GetOverlappingBodies();

		foreach (var body in initialOverlaps)
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
		var spaceState = _owner.GetWorld3D().DirectSpaceState;
		var targetCharacterBody = targetCharacter.CharacterBody;
		var traceOrigin = _owner.Eyes.GlobalPosition;
		var traceEnd = targetCharacter.Chest.GlobalPosition;
		var traceResult = Trace.Line(spaceState, traceOrigin, targetCharacterBody.GlobalPosition, _exclude);

		if (_debug)
		{
			DebugDraw.Line(traceOrigin, traceEnd, Colors.Red);
		}

		return traceResult != null && traceResult.Collider.Equals(targetCharacter);
	}

	private void CheckShouldObserveBody(Node3D body)
	{
		if (body is CharacterBase c && c != _owner)
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

		if (!_debug) return true;
		
		GD.Print(character.Name + " entered sight radius of " + GetOwner<Node>().Name);
		DebugDraw.Sphere(character.GlobalTransform, 1, Colors.Red, 3f);

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

		if (!_debug) return true;
		GD.Print(character.Name + " exited sight radius of " + GetOwner<Node>().Name);
		DebugDraw.Sphere(character.GlobalTransform, 1, Colors.Red, 3f);

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

		if (!_debug) return true;
		GD.Print(character.Name + " entered line of sight of " + GetOwner<Node>().Name);

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

		if (!_debug) return true;
		GD.Print(character.Name + " exited line of sight of " + GetOwner<Node>().Name);
		return true;
	}
}
