using Godot;

namespace ProjectMina;

// got this trick from GTAIII source code
// placing count at the end of the enum lets us easily keep track of the count without storing another variable
public enum CharacterFaction : int
{
	Civ,
	Guard,
	Thief,
	Count
}

[GlobalClass]
public partial class CharacterBase : CharacterBody3D
{
	// signals
	[Signal] public delegate void AttackedEventHandler();
	[Signal] public delegate void FinishedAttackEventHandler();
	[Signal]  public delegate void FocusChangedEventHandler(Vector3 focusPosition);

	// publicly accessible components
	[Export] public CollisionShape3D CharacterBody { get; set; }
	[Export] public HealthComponent CharacterHealth { get; protected set; }
	[Export] public AttentionComponent CharacterAttention { get; protected set; }
	[Export] public MovementComponent CharacterMovement { get; protected set; }
	[Export] public InteractionComponent CharacterInteraction { get; protected set; }
	[Export] public EquipmentComponent CharacterEquipment { get; protected set; }
	[Export] public SoundComponent CharacterSound { get; protected set; }
	[Export] public AnimationPlayer CharacterAnimationPlayer { get; protected set; }
	[Export] public AnimationTree CharacterAnimationTree { get; protected set; }
	[Export] public Marker3D Eyes { get; protected set; }
	[Export] public Marker3D Chest { get; protected set; }
	[Export] public CharacterFaction Faction { get; protected set; }

	[Export] protected double RotationRate = 6.0;

	[Export] protected bool _debug = false;

	public Vector3 ForwardVector { get; protected set; }

	protected double _footstepTimeout = .1;
	protected bool _canFootstep = true;
	private Timer _footstepTimer;

	protected PhysicsMaterial _floorSurface;

	protected Node3D _lookTarget;
	protected Vector3 _focusPosition;

	public override void _Ready()
	{
		if (Engine.IsEditorHint())
		{
			return;
		}

		_footstepTimer = new()
		{
			WaitTime = .5,
			Autostart = false,
			OneShot = true
		};

		_footstepTimer.Timeout += () =>
		{
			GD.Print("footstep timeout");
			_canFootstep = true;
		};

		AddChild(_footstepTimer);

		CharacterHealth.HealthDepleted += Die;
	}

	public override void _PhysicsProcess(double delta)
	{
		ForwardVector = -GlobalTransform.Basis.Z;
	}

	public virtual void SetLookTarget(Node3D target)
	{
		if (target == null)
		{
			ClearLookTarget();
			return;
		}
		
		_lookTarget = target;
	}

	public virtual void ClearLookTarget()
	{
		_lookTarget = null;
	}

	// used at least by AI characters to set their look position
	public virtual void SetFocus(Node3D focus)
	{
		if (focus == null)
		{
			ClearFocus();
			return;
		}
		
		GD.Print("focus changed");
		_focusPosition = focus.GlobalPosition;
		EmitSignal(SignalName.FocusChanged, _focusPosition);
	}
	
	public virtual void SetFocus(Vector3 focusPosition)
	{
		_focusPosition = focusPosition;
		EmitSignal(SignalName.FocusChanged, _focusPosition);
	}

	public virtual void ClearFocus()
	{
		_focusPosition = Vector3.Zero;
		EmitSignal(SignalName.FocusChanged, _focusPosition);
	}

	// intended to be called by the logic of the entity controlling the character; the ai brain or the player character
	public virtual void Attack()
	{
		EmitSignal(SignalName.Attacked);
	}

	// typically triggered by animations signalling that the character is able to attack again
	public virtual bool FinishAttack()
	{
		EmitSignal(SignalName.FinishedAttack);
		return false;
	}

	public virtual void Die()
	{
		if (_debug)
		{
			DebugDraw.Text(Name + " is dead.");
		}

		QueueFree();
	}

	public virtual void Footstep()
	{
		GD.Print("footstep!");
	}
	
	public bool HasLineOfSight(Node3D target)
	{
		
		var res = Cast.Ray(GetWorld3D().DirectSpaceState, Eyes.GlobalPosition, target.GlobalPosition, new() { this.GetRid() });

		return res?.Collider == null || res.Collider == target || res.Collider == target.GetOwner<Node3D>();
	}

	protected virtual PhysicsMaterial GetFloorSurface(Vector3 traceOrigin = new())
	{
		var spaceState = GetWorld3D().DirectSpaceState;
		if (traceOrigin == Vector3.Zero)
		{
			traceOrigin = GlobalPosition;
		}
		
		Vector3 traceEnd = traceOrigin + (Vector3.Down * 2);
		HitResult res = Cast.Ray(spaceState, traceOrigin, traceEnd, new() { this.GetRid() });

		PhysicsMaterial surfaceMaterial = new();

		if (res != null)
		{
			return surfaceMaterial;
		}
		
		switch (res.Collider)
		{
			case StaticBody3D staticBody:
				surfaceMaterial = staticBody.PhysicsMaterialOverride;
				break;
			case RigidBody3D rigidBody:
				surfaceMaterial = rigidBody.PhysicsMaterialOverride;
				break;
		}

		return surfaceMaterial;
	}
}
