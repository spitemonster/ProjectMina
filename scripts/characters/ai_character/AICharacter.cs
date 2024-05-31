using System;
using System.Diagnostics;
using Godot;
using ProjectMina.BehaviorTree;
namespace ProjectMina;

[GlobalClass]
public partial class AICharacter : CharacterBase
{
	// [Export] public AgentComponent Brain;
	[Export] public float BrakingDistance = 1.0f;
	[Export] public SearchComponent SearchComponent { get; protected set; }
	[Export] public NavigationAgent3D NavigationAgent { get; protected set; }
	[Export] public SteeringComponent Steering { get; protected set; }
	[Export] public AIPerceptionComponent CharacterPerception { get; protected set; }

	[Export] protected float NoticeThreshold = 25;
	[Export] protected float AlertThreshold = 15;

	protected float MinimumVelocity = 0.1f;
	
	private Vector3 _direction = new();

	// private Timer _noticeTimer;

	private CharacterBase _targetCharacter;
	private bool _targetCharacterNoticed;
	private bool _targetCharacterSeen;
	private float _noticeAmount = 0.0f;
	private float _alertAmount = 0.0f;

	[Export] protected ProgressBar NoticeBar;
	[Export] protected ProgressBar AlertBar;

	public void EquipWeapon(EquippableComponent weapon)
	{
		CharacterEquipment.Equip(weapon);
	}

	public void UseFocusedInteractable()
	{
		// var focus = (Node3D)Brain.Blackboard.GetValue("current_focus");
		// var interactable = focus.GetNode<InteractableComponent>("Interactable");
		// interactable.Interact(this);
	}

	public void FinishInteractableAnim()
	{
		CharacterAnimationTree.RemoveAnimationLibrary("interactable");
	}

	public void SetTargetPosition(Vector3 targetPosition)
	{
		NavigationAgent.TargetPosition = targetPosition;
	}

	public override void _Ready()
	{
		base._Ready();

		NoticeBar.MaxValue = NoticeThreshold;
		AlertBar.MaxValue = AlertThreshold;
		
		NavigationAgent.DebugEnabled = true;
		NavigationAgent.VelocityComputed += SetSafeVelocity;
		
		CharacterHealth.HealthChanged += (double newHealth, bool wasDamage) =>
		{
		};

		CharacterMovement.EnableClimbing = false;
		CharacterMovement.EnableJumping = false;
		CharacterMovement.EnableSneaking = false;

		Global.Data.AddAICharacter(this);
	}

	private void _StartNoticeTimer(CharacterBase character)
	{
		if (_targetCharacter != null)
		{
			return;
		}
		
		_targetCharacter = character;
	}

	private void _ClearNoticeTimer()
	{
		if (_targetCharacter == null)
		{
			return;
		}

		_targetCharacterNoticed = false;
		_targetCharacterSeen = false;
		_targetCharacter = null;
		_alertAmount = 0.0f;
		
		_noticeAmount = 0.0f;
		NoticeBar.SetValue(0);
		AlertBar.SetValue(0);
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		if (_targetCharacter != null && !_targetCharacterSeen)
		{
			float visibility = 1.0f;
			float distance = GlobalPosition.DistanceTo(_targetCharacter.GlobalPosition);
			float distanceRatio =  Mathf.Clamp(1 - (distance / CharacterPerception.MaxSightDistance), 0, 1);
			float speed = _targetCharacter.Velocity.Length();
			float speedRatio = Mathf.Clamp(_targetCharacter.Velocity.Length() / 1.3f, 0, 1);
			float sneakMultiplier = _targetCharacter.CharacterMovement.IsSneaking ? .6f : 1.0f;
			
			if (_targetCharacter.GetNode("VisibilityComponent") is VisibilityComponent v)
			{
				visibility = v.GetVisibility();
			}

			if (!_targetCharacterNoticed)
			{
				float noticeAmount = (((visibility * 5f) + (distanceRatio * 4f) + (speedRatio * 2f)) * (float)delta) * sneakMultiplier;
				_noticeAmount += noticeAmount;
				NoticeBar.SetValue(_noticeAmount);
				
				if (_noticeAmount > NoticeThreshold)
				{
					_targetCharacterNoticed = true;
				}
			} else
			{
				float alertAmount = (((visibility * 5f) + (distanceRatio * 4f) + (speedRatio * 2f)) * (float)delta) * sneakMultiplier;
				_alertAmount += alertAmount;
				AlertBar.SetValue(_alertAmount);

				if (_alertAmount >= AlertThreshold)
				{
					_targetCharacterSeen = true;
				}
			}
		}
		
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

		if (_targetCharacter != null && _targetCharacterSeen)
		{
			SetTargetPosition(_targetCharacter.GlobalPosition);
		}
		
		_direction = (NavigationAgent.GetNextPathPosition() - GlobalPosition).Normalized();
		
		Vector3 localVelocity;

		if (!NavigationAgent.IsTargetReached() && _direction != Vector3.Zero)
		{
			localVelocity = CharacterMovement.GetCharacterVelocity(_direction, delta, GetWorld3D().DirectSpaceState);
		}
		else
		{
			localVelocity = CharacterMovement.GetCharacterVelocity(Vector3.Zero, delta, GetWorld3D().DirectSpaceState);
		}
		
		if (Steering != null)
		{
			Vector3 steeredVelocity = Steering.CalculateSteeringVelocity(localVelocity);
			localVelocity = localVelocity.MoveToward(steeredVelocity, 1f);
		}

		if (NavigationAgent.AvoidanceEnabled)
		{
			NavigationAgent.Velocity = localVelocity;
		}
		else
		{
			SetSafeVelocity(localVelocity);
		}
	}

	private void SetSafeVelocity(Vector3 safeVelocity)
	{
		Velocity = safeVelocity;
		CharacterAnimationTree.Set("parameters/locomotion/blend_position", Velocity.Length());
		
		// we want to get the look direction and the movement direction because in the animation tree they will control different things
		// look direction is easy; it is the direction to the look target
		Vector3 lookPosition = Vector3.Zero;

		if (_targetCharacter != null && _targetCharacterNoticed)
		{
			lookPosition = _targetCharacter.GlobalPosition;
		}
		// rotate to face the current look target
		else if (LookTarget != null)
		{
			lookPosition = LookTarget.GlobalPosition;
		}
		// if there is no current look target, rotate to face direction of movement
		else if (Velocity.Length() > 0.1)
		{
			lookPosition = GlobalPosition + Velocity.Normalized();
		}
		
		Vector3 direction = (lookPosition - GlobalPosition).Normalized();

		// only rotate if the look position is set and it's not our current position and the direction is not parallel to the up vector
		if (lookPosition != Vector3.Zero && lookPosition != GlobalPosition && Math.Abs(direction.Dot(Vector3.Up)) < 0.99f)
		{
			GlobalTransform = GlobalTransform.InterpolateWith(GlobalTransform.LookingAt(lookPosition, Vector3.Up), 0.05f);
			GlobalRotation *= new Vector3(0, 1, 0);
		}
	}
}
