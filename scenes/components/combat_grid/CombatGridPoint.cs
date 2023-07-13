using Godot;
using Godot.Collections;

namespace ProjectMina;
[GlobalClass]
public partial class CombatGridPoint : Node3D
{
	public enum CombatType
	{
		Melee,
		Ranged
	};

	public bool Occupied { get; private set; } = false;
	public CharacterBase OccupyingCharacter { get; protected set; }

	[Signal]
	public delegate void PointFreedEventHandler();

	[Export]
	protected Area3D _collisionArea;

	[Export]
	public CombatType combatType;

	public bool CanOccupy()
	{
		if (_collisionArea == null || Occupied)
		{
			return false;
		}

		Debug.Assert(_collisionArea != null, "no collision area");
		Array<Node3D> overlappingBodies = _collisionArea.GetOverlappingBodies();
		return overlappingBodies.Count == 0;
	}

	public void OccupyPoint(CharacterBase character)
	{
		Occupied = true;
		OccupyingCharacter = character;
		OccupyingCharacter.CharacterHealthComponent.HealthDepleted += FreePoint;
	}

	private void FreePoint()
	{
		Occupied = false;
		OccupyingCharacter.CharacterHealthComponent.HealthDepleted -= FreePoint;
		OccupyingCharacter = null;
	}
}
