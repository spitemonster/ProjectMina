using Godot;
using Godot.Collections;
namespace ProjectMina;

/// <summary>
/// 	Test!
/// </summary>

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
	public Array<CharacterBase> SeenCharacters { get; private set; } = new();

	[Signal] public delegate void CharacterEnteredSightRadiusEventHandler(CharacterBase character);
	[Signal] public delegate void CharacterExitedSightRadiusEventHandler(CharacterBase character);
	[Signal] public delegate void CharacterEnteredLineOfSightEventHandler(CharacterBase character);
	[Signal] public delegate void CharacterExitedLineOfSightEventHandler(CharacterBase character);
	[Export] protected Area3D _sightCollision;

	[Export] private bool debug = false;
	private Array<Rid> exclude = new();
	private int _visibilityCheckIndex;

	public override void _Ready()
	{
		if (debug)
		{
			System.Diagnostics.Debug.Assert(_sightCollision != null, "no sight collision");
		}

		_sightCollision.BodyEntered += CheckShouldObserveBody;
		_sightCollision.BodyExited += CheckRemoveBody;
		exclude.Add(GetOwner<CharacterBase>().GetRid());

		CallDeferred("CheckInitialOverlaps");

	}

	public override void _PhysicsProcess(double delta)
	{
		foreach (CharacterBase c in CharactersInSightRadius)
		{
			CheckCharacterLineOfSight(c);
		}
		// if (CharactersInSightRadius.Count > 0 && CharactersInSightRadius[_visibilityCheckIndex] is CharacterBase visibilityCheckTarget)
		// {
		// 	CheckCharacterVisibility(visibilityCheckTarget);
		// 	CalculateCharacterVisibility(visibilityCheckTarget);
		// }
		// else if (_visibilityCheckIndex > 0)
		// {
		// 	_visibilityCheckIndex = 0;
		// }
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
			if (body is CharacterBase c)
			{
				TryAddCharacterToSightRadius(c);
			}
		}
	}

	private void CheckCharacterLineOfSight(CharacterBase visibilityCheckTarget)
	{
		// insert some kind of check to see if we even care to watch this character
		PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
		CollisionShape3D targetCharacterBody = visibilityCheckTarget.CharacterBody;

		if (debug)
		{
			DebugDraw.Line(GlobalPosition, targetCharacterBody.GlobalPosition, Colors.Red);
		}

		HitResult traceResult = Trace.Line(spaceState, GlobalPosition, targetCharacterBody.GlobalPosition, exclude);

		if (traceResult != null && traceResult.Collider.Equals(visibilityCheckTarget))
		{
			if (debug)
			{
				DebugDraw.Sphere(targetCharacterBody.GlobalTransform, .5f, Colors.Red);
			}

			TryAddSeenCharacter(visibilityCheckTarget);
		}
		else
		{
			TryRemoveSeenCharacter(visibilityCheckTarget);
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

		if (debug)
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

		if (debug)
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

		if (debug)
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

		if (debug)
		{
			GD.Print(character.Name + " exited line of sight of " + GetOwner<Node>().Name);
		}
		return true;
	}
}
