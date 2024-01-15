using System.Diagnostics;
using Godot;
using ProjectMina.BehaviorTree;
namespace ProjectMina;

public enum AIState
{
	Idle,
	Suspicious,
	Alerted
}

public enum AIBehavior
{
	Patrol,
	Search,
	Attack,
	Wait
}

[GlobalClass]
public partial class AICharacter : CharacterBase
{
	[Export] public AIBrainComponent Brain;
	[Export] public float BrakingDistance = 1.0f;
	[Export] public SearchComponent SearchComponent { get; protected set; }
	[Export] public NavigationAgent3D NavigationAgent { get; protected set; }
	
	private Vector3 _direction = new();
	private Vector3 _lookTarget = new();

	public void EquipWeapon(EquippableComponent weapon)
	{
		CharacterEquipment.Equip(weapon);
	}

	public bool HasLineOfSight(Node3D target)
	{
		// if (_debug)
		// {
		// 	GD.Print("testing AI line of sight to: ", target.Name);	
		// }
		
		HitResult res = Cast.Ray(GetWorld3D().DirectSpaceState, Eyes.GlobalPosition, target.GlobalPosition, new() { this.GetRid() });

		if (res.Collider == null || res.Collider == target)
		{
			return true;
		}

		return false;
	}

	public void UseFocusedInteractable()
	{
		var focus = (Node3D)Brain.Blackboard.GetValue("current_focus");
		var interactable = focus.GetNode<InteractableComponent>("Interactable");
		interactable.Interact(this);
	}

	public void FinishInteractableAnim()
	{
		CharacterAnimationTree.RemoveAnimationLibrary("interactable");
	}

	public override void _Ready()
	{
		base._Ready();
		
		System.Diagnostics.Debug.Assert(Brain != null, "no brain component");
		System.Diagnostics.Debug.Assert(NavigationAgent != null, "No navigation agent");
		System.Diagnostics.Debug.Assert(CharacterMovement != null, "No movement component");
		
		NavigationAgent.DebugEnabled = true;
		
		CharacterHealth.HealthChanged += (double newHealth, bool wasDamage) =>
		{
			GD.Print("Character Health Changed");
		};

		CharacterMovement.EnableClimbing = false;
		CharacterMovement.EnableJumping = false;
		CharacterMovement.EnableSneaking = false;
		
		NavigationAgent.VelocityComputed += SetVelocity;
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
		
		for (var i = 0; i < GetSlideCollisionCount(); i++)
		{
			KinematicCollision3D collision3D = GetSlideCollision(i);
			if (collision3D.GetCollider() is not RigidBody3D r || !IsOnFloor()) continue;
			
			var directionToCollision = (collision3D.GetPosition() - CharacterBody.GlobalPosition).Normalized();
			var angleToCollision = new Vector3(0.0f, -1.0f, 0.0f).AngleTo(directionToCollision);
			var angleFactor = Mathf.Clamp(Mathf.Pow(angleToCollision, 2), 0.0f, 1.0f);
			angleFactor = Mathf.Round(angleFactor * 100) / 100;
		
			if (!(angleFactor > 0.5)) continue;
			
			r.ApplyCentralImpulse(-collision3D.GetNormal() * 4.0f * angleFactor);
			r.ApplyImpulse(-collision3D.GetNormal() * 0.01f * angleFactor, collision3D.GetPosition());
		}

		_direction = (NavigationAgent.TargetPosition - GlobalPosition).Normalized();
		_lookTarget = NavigationAgent.TargetPosition;
		
		var dist = (NavigationAgent.TargetPosition - GlobalPosition).Length() - .5f;
		var mult = Mathf.Clamp((dist / BrakingDistance), 0.0f, 1.0f);

		if (!NavigationAgent.IsTargetReached())
		{
			// Velocity = CharacterMovement.GetCharacterVelocity(direction, delta, GetWorld3D().DirectSpaceState) * mult;
			NavigationAgent.Velocity = CharacterMovement.GetCharacterVelocity(_direction, delta, GetWorld3D().DirectSpaceState); 
		}
		else
		{
			// Velocity = CharacterMovement.GetCharacterVelocity(Vector3.Zero, delta, GetWorld3D().DirectSpaceState) * mult;
			NavigationAgent.Velocity = CharacterMovement.GetCharacterVelocity(Vector3.Zero, delta, GetWorld3D().DirectSpaceState);
		}
	}

	private void SetVelocity(Vector3 safeVelocity)
	{
		Velocity = safeVelocity;
		CharacterAnimationTree.Set("parameters/test/blend_position", Velocity.Length());
		
		if (_lookTarget != null)
		{
			var t = GlobalTransform;
			t = t.InterpolateWith(t.LookingAt(_lookTarget, Vector3.Up), 0.05f);
			GlobalTransform = t;

			var gr = GlobalRotation;
			gr.X = 0;
			gr.Z = 0;
			GlobalRotation = gr;
		}
		
		MoveAndSlide();
	}
}
