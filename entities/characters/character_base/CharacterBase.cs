using Godot;

namespace ProjectMina;

[GlobalClass]
public partial class CharacterBase : CharacterBody3D
{
	// signals
	[Signal] public delegate void ControllerUpdatedEventHandler(ControllerComponent controller);
	[Signal] public delegate void AttackedEventHandler();
	[Signal] public delegate void FinishedAttackEventHandler();
	[Signal] public delegate void FocusChangedEventHandler(Node3D newFocus, Node3D previousFocus);
	[Signal] public delegate void FocusBrokenEventHandler(Node3D previousFocus);
	[Signal] public delegate void CharacterSteppedEventHandler(PhysicsMaterial floorMaterial);
	
	// input on the movement axis, or the direction the character should move
	public Vector2 MovementInput { get; protected set; }
	// input on on the control axis, or the direction the character should look
	public Vector2 ControlInput { get; protected set; }
	
	[ExportSubgroup("Controller")]
	[Export] private PackedScene _defaultControllerScene;

	// publicly accessible components
	[ExportSubgroup("Physicality")]
	[Export] public CollisionShape3D Body { get; set; }
	[Export] public Node3D Head { get; protected set; }
	[Export] public Marker3D Chest { get; protected set; }
	
	[Export] protected float Mass = 80.0f;
	[Export] protected float PushForce = 5.0f;
	// [Export] public Marker3D Eyes { get; protected set; }
	
	[ExportSubgroup("Movement")]
	[Export] protected double RotationRate = 6.0;
	[Export] public bool DisableGravity = false;
	[Export] protected float MaxStepHeight = 0.5f;
	[ExportSubgroup("Sprint")]
	[Export] protected bool SprintEnabled;
	[ExportSubgroup("Crouch")]
	[Export] protected bool CrouchEnabled;
	
	[ExportSubgroup("CharacterComponents")]
	[Export] public HealthComponent CharacterHealth { get; protected set; }
	// [Export] public AttentionComponent CharacterAttention { get; protected set; }
	[Export] public MovementComponent CharacterMovement { get; protected set; }
	[Export] public InteractionComponent CharacterInteraction { get; protected set; }
	[Export] public EquipmentComponent CharacterEquipment { get; protected set; }
	[Export] public AnimationPlayer AnimPlayer { get; protected set; }
	[Export] public CharacterAnimationTree AnimTree { get; protected set; }
	
	[ExportSubgroup("Gameplay")]
	[Export] public ECharacterFaction Faction { get; protected set; }
	[Export] protected bool _debug = false;

	public bool CanMove { get; protected set; } = true;

	public bool Dead { get; protected set; } = false;
	public StringName ControllerType => Controller.GetClass();
	public Vector3 ForwardVector { get; private set; }

	protected bool CanFootstep = true;

	protected Node3D LookTarget;
	protected Vector3 FocusPosition;
	
	// for stairs
	protected RayCast3D StairsAheadTrace;
	protected RayCast3D StairsBelowTrace;
	protected bool SnappedToStairsLastFrame;
	protected ulong LastFrameOnFloor;
	
	public PhysicsBody3D CurrentFloor { get; protected set; }
	public PhysicsMaterial CurrentFloorMaterial { get; protected set; }
	
	public ControllerComponent Controller { get; protected set; }

	protected bool CanAttack = true;

	public void DisableMovement()
	{
		CanMove = false;
	}

	public void EnableMovement()
	{
		CanMove = true;
	}

	// sets the controller that controls this character in the world
	public bool SetController(ControllerComponent controller)
	{
		if (Controller != null)
		{
			return false;
		}
		
		Controller = controller;
		EmitSignal(SignalName.ControllerUpdated, Controller);
		return true;
	}
	
	// methods used by the controller to control this character
	public virtual void SetControlInput(Vector2 input)
	{
		ControlInput = input;
	}

	public virtual void SetMovementInput(Vector2 input)
	{
		MovementInput = input;
	}

	public virtual bool CanSprint()
	{
		return SprintEnabled && IsOnFloor();
	}
	
	public virtual void SetLookTarget(Node3D target)
	{
		if (target == null)
		{
			ClearLookTarget();
			return;
		}
		
		LookTarget = target;
	}

	public virtual void ClearLookTarget()
	{
		LookTarget = null;
	}

	// used at least by AI characters to set their look position
	public virtual void SetFocus(Node3D focus)
	{
		if (focus == null)
		{
			ClearFocus();
			return;
		}
		
		FocusPosition = focus.GlobalPosition;
	}
	
	public virtual void SetFocus(Vector3 focusPosition)
	{
		FocusPosition = focusPosition;
	}

	public virtual void ClearFocus()
	{
		FocusPosition = Vector3.Zero;
	}

	public virtual void EnableAttack()
	{
		CanAttack = true;
	}

	public virtual void DisableAttack()
	{
		CanAttack = false;
	}
	
	public virtual void Attack()
	{
		EmitSignal(SignalName.Attacked);
	}
	
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

		Dead = true;

		// QueueFree();
	}

	public virtual void Footstep()
	{
		EmitSignal(SignalName.CharacterStepped, CurrentFloorMaterial);
	}

	public virtual float GetVisibility()
	{
		return 1.0f;
	}

	public virtual float GetHealth()
	{
		if (CharacterHealth != null)
		{
			return (float)CharacterHealth.CurrentHealth;
		}
		
		return 100.0f;
	}
	
	public bool HasLineOfSight(Node3D target)
	{
		
		var res = Cast.Ray(GetWorld3D().DirectSpaceState, Head.GlobalPosition, target.GlobalPosition, new() { GetRid() });

		return res?.Collider == null || res.Collider == target || res.Collider == target.GetOwner<Node3D>();
	}

	public override void _Ready()
	{
		if (Engine.IsEditorHint())
		{
			return;
		}

		// create
		if (_defaultControllerScene != null && GetNodeOrNull("%Controller") == null)
		{
			CreateDefaultController();
		}

		CharacterHealth.HealthDepleted += Die;
		
		StairsAheadTrace = GetNodeOrNull<RayCast3D>("%StairsAheadTrace");
		StairsBelowTrace = GetNodeOrNull<RayCast3D>("%StairsBelowTrace");
	}

	protected virtual void CreateDefaultController()
	{
		var controllerScene = ResourceLoader.Load<PackedScene>(_defaultControllerScene.ResourcePath);

		ControllerComponent controller = controllerScene.Instantiate<ControllerComponent>();

		if (controller != null)
		{
			AddChild(controller);
			controller.Possess(this);
		}
	}

	public override void _Process(double delta)
	{
		ForwardVector = -GlobalTransform.Basis.Z;
		UpdateCurrentFloor();
	}

	public override void _PhysicsProcess(double delta)
	{
		ApplyRigidBodyCollisions();
		MoveAndSlide();	
		// if (StairsAheadTrace == null || StairsBelowTrace == null)
		// {
		// 	MoveAndSlide();	
		// } else if (!SnapUpToStairs(delta))
		// {
		// 	MoveAndSlide();	
		// 	SnapDownToStairs();
		// }
		
		_ApplyGravity(delta);
		// UpdateCurrentFloor();
		if (IsOnFloor() || SnappedToStairsLastFrame)
		{
			LastFrameOnFloor = Engine.GetPhysicsFrames();
		}
	}

	protected void SnapDownToStairs()
	{
		bool snapped = false;
		bool wasOnFloorLastFrame = Engine.GetPhysicsFrames() - LastFrameOnFloor == 1;
		bool stairsBelow = StairsBelowTrace.IsColliding() && !_IsFloorTooSteep(StairsBelowTrace.GetCollisionNormal());
		if (!IsOnFloor() && Velocity.Y < 0 && (wasOnFloorLastFrame || SnappedToStairsLastFrame) && stairsBelow)
		{
			var motionTestResult = _RunMotionTest(GlobalTransform, new Vector3(0, -MaxStepHeight, 0));
			
			if (motionTestResult == null)
			{
				return;
			}
			
			var pos = GlobalPosition;
			pos.Y += motionTestResult.GetTravel().Y;
			GlobalPosition = pos;
			ApplyFloorSnap();
			snapped = true;
		}

		SnappedToStairsLastFrame = snapped;
	}

	protected bool SnapUpToStairs(double delta)
	{
		if ((!IsOnFloor() && !SnappedToStairsLastFrame) || Velocity.Y > 0 || (Velocity * new Vector3(1, 0, 1)).Length() == 0.0f)
		{
			return false;
		}
		
		var motionDirection = Velocity.Normalized() * new Vector3(1, 0, 1);
		StairsAheadTrace.GlobalPosition = GlobalPosition + new Vector3(0, .5f, 0) + motionDirection * 1f;
		StairsAheadTrace.ForceRaycastUpdate();

		if (!StairsAheadTrace.IsColliding() || _IsFloorTooSteep(StairsAheadTrace.GetCollisionNormal()))
		{
			return false;
		}
		
		var stepPosWithClearance = GlobalTransform.Translated(motionDirection * .1f + new Vector3(0, .5f, 0));
		
		DebugDraw.Sphere(stepPosWithClearance.Origin, .5f, Colors.Red);
		
		var motionTestResult = _RunMotionTest(stepPosWithClearance, new Vector3(0, -MaxStepHeight * 2, 0));
		
		if (motionTestResult == null)
		{
			return false;
		}

		var stepHeight = (stepPosWithClearance.Origin + motionTestResult.GetTravel() - GlobalPosition).Y;
		
		if (stepHeight < .09)
		{
			return false;
		}
		
		var newPos = stepPosWithClearance.Origin + motionTestResult.GetTravel() + new Vector3( 0f, .25f, 0f);
		DebugDraw.Sphere(newPos, .25f, Colors.Green);
		GlobalPosition = newPos;
		ApplyFloorSnap();
		SnappedToStairsLastFrame = true;
		
		return true;
		//
		// var stepHeight = (stepPosWithClearance.Origin + motionTestResult.GetTravel() - GlobalPosition).Y;
		// //
		// //
		// if (stepHeight > MaxStepHeight || stepHeight <= 0.02 || (motionTestResult.GetCollisionPoint() - GlobalPosition).Y > MaxStepHeight)
		// {
		// 	GD.Print("this. step height > max step height: ", stepHeight > MaxStepHeight, ". step heit < .2: ", stepHeight <= 0.02, ". other thing: ", (motionTestResult.GetCollisionPoint() - GlobalPosition).Y > MaxStepHeight);
		// 	return false;
		// }
		//
		// StairsAheadTrace.GlobalPosition = motionTestResult.GetCollisionPoint() + new Vector3(0, MaxStepHeight, 0) + expectedMoveMotion.Normalized() * .1f;
		// StairsAheadTrace.ForceRaycastUpdate();
		//
		//
		// if (StairsAheadTrace.IsColliding() && !_IsFloorTooSteep(StairsAheadTrace.GetCollisionNormal()))
		// {
		// 	GlobalPosition = stepPosWithClearance.Origin + motionTestResult.GetTravel() + new Vector3(0, .1f, 0);
		// 	ApplyFloorSnap();
		// 	SnappedToStairsLastFrame = true;
		// 	return true;
		// }
		//
		return false;
	}

	// get the material at the character's feet, set the current floor and current physics floor material
	protected void UpdateCurrentFloor()
	{
		var spaceState = GetWorld3D().DirectSpaceState;
		if (!IsOnFloor() && !SnappedToStairsLastFrame && CurrentFloor != null)
		{
			if (CurrentFloor is RigidBody3D r)
			{
				r.Sleeping = false;
			}
			CurrentFloor = null;
			CurrentFloorMaterial = null;
		}
		else
		{
			var traceResult = Cast.Ray(spaceState, GlobalPosition, GlobalPosition + Vector3.Up * -.5f, new() { GetRid() });

			if (traceResult == null || traceResult.Collider is not StaticBody3D or RigidBody3D)
			{
				return;
			}
			
			if (traceResult.Collider is RigidBody3D r)
			{
				r.Sleeping = true;
			}

			CurrentFloor = traceResult.Collider;
			CurrentFloorMaterial = Utilities.GetPhysicsMaterialFromPhysicsBody(CurrentFloor);
		}
	}

	// push against rigidbodies with which we are colliding
	protected void ApplyRigidBodyCollisions()
	{
		if (!IsOnFloor() && !SnappedToStairsLastFrame)
		{
			return;
		}
		
		for (var i = 0; i < GetSlideCollisionCount(); i++)
		{
			KinematicCollision3D collision3D = GetSlideCollision(i);
			if (collision3D.GetCollider() is not RigidBody3D r || r == CurrentFloor) continue;
			
			var collisionDirection = -collision3D.GetNormal();
			var directionToCollision = (r.GlobalPosition - Body.GlobalPosition).Normalized();
			var angleToCollision = new Vector3(0.0f, -1.0f, 0.0f).AngleTo(directionToCollision);
			var diff = Velocity.Dot(collisionDirection) - r.LinearVelocity.Dot(collisionDirection);
			
			diff = Mathf.Max(0, diff);

			var massRatio = Mathf.Min(1, Mass / r.Mass);
			collisionDirection.Y = 0;
			
			var angleFactor = Mathf.Clamp(Mathf.Pow(angleToCollision, 2), 0.0f, 1.0f);
			angleFactor = Mathf.Round(angleFactor * 100) / 100;

			if (!(angleFactor > 0.5)) continue;
			
			r.ApplyImpulse(collisionDirection * diff * massRatio * PushForce * angleFactor, collision3D.GetPosition() - r.GlobalPosition);
		}
	}

	private bool _IsFloorTooSteep(Vector3 normal)
	{
		return normal.AngleTo(Vector3.Up) > FloorMaxAngle;
	}

	private void _ApplyGravity(double delta)
	{
		if (DisableGravity)
		{
			return;
		}

		var vel = Velocity;
		vel.Y -= (float)ProjectSettings.GetSetting("physics/3d/default_gravity") * (float)delta;

		Velocity = vel;
	}
	
	private PhysicsTestMotionResult3D _RunMotionTest(Transform3D origin, Vector3 motion)
	{
		var result = new PhysicsTestMotionResult3D();
		var testParams = new PhysicsTestMotionParameters3D();

		testParams.From = origin;
		testParams.Motion = motion;

		if (PhysicsServer3D.BodyTestMotion(GetRid(), testParams, result))
		{
			if (result.GetCollider() is not StaticBody3D && result.GetCollider() is not CsgShape3D)
			{
				return null;
			}

			return result;
		}

		return null;
	}
}
 