using Godot;

namespace ProjectMina;

[GlobalClass]
public partial class CharacterBase : CharacterBody3D
{

	[Export]
	public CollisionShape3D CharacterBody { get; protected set; }
	[Export]
	public HealthComponent CharacterHealthComponent { get; protected set; }
	public Vector3 ForwardVector { get; private set; }

	public override void _PhysicsProcess(double delta)
	{
		ForwardVector = -GlobalTransform.Basis.Z;
	}
}