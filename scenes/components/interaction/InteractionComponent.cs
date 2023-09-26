using Godot;

namespace ProjectMina;

[GlobalClass]
public partial class InteractionComponent : ComponentBase
{
	public enum InteractionType
	{
		None,
		Grab,
		Use,
		Equip
	};

	public bool CanGrab { get => _canGrab; }
	public bool CanEquip { get => _canEquip; }
	public bool CanInteract { get => _canInteract; }
	public bool IsGrabbing { get => _grabbedItem != null; }

	public Node3D FocusedItem { get => _focusedItem; }

	[Signal]
	public delegate void FocusedEventHandler(Node3D item);
	[Signal]
	public delegate void LostFocusEventHandler(Node3D item);
	[Signal]
	public delegate void InteractedEventHandler(Node3D item);
	[Signal]
	public delegate void GrabbedEventHandler(RigidBody3D item);
	[Signal]
	public delegate void DroppedEventHandler(RigidBody3D item, bool thrown);
	[Signal]
	public delegate void InteractionStateChangedEventHandler(InteractionType newState);

	[Export]
	protected Area3D _interactionCollision;
	[Export]
	protected Marker3D _grabPosition;
	[Export]
	protected Generic6DofJoint3D _grabJoint;
	[Export]
	protected StaticBody3D _grabAnchor;
	[Export]
	protected float _grabStrength = 20.0f;
	[Export]
	protected float _grabRotationStrength = 2.0f;
	[Export]
	protected EquipmentManager equipmentManager;

	private bool _canInteract;
	private bool _canEquip;
	private bool _canGrab;
	private Node3D _focusedItem;
	private RigidBody3D _grabbedItem;
	private InputManager _inputManager;
	private Quaternion _grabbedItemDesiredRotation;
	private InteractionType _interactionState = InteractionType.None;

	public override void _Ready()
	{
		base._Ready();

		if (!_active || Engine.IsEditorHint())
		{
			return;
		}

		PlayerCharacter playerCharacter = GetOwner<PlayerCharacter>();

		if (playerCharacter != null && playerCharacter.CharacterAttention != null)
		{
			playerCharacter.CharacterAttention.FocusChanged += (newFocus, previousFocus) =>
			{

				if (newFocus == null)
				{
					if (_focusedItem != null)
					{
						LoseFocus();
						return;
					}
				}
				else
				{
					CheckInteraction(newFocus);
				}
			};
		}

		Debug.Assert(_grabJoint != null, "no grab joint");
		Debug.Assert(_grabAnchor != null, "no grab anchor");
	}

	public void Interact(bool isAlt)
	{
		switch (_interactionState)
		{
			case InteractionType.Grab:
				Grab((RigidBody3D)_focusedItem);
				break;
			case InteractionType.Use:
				Use(_focusedItem);
				break;
			case InteractionType.Equip:
				equipmentManager.EquipItem(_focusedItem);
				break;
		}
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		if (_grabbedItem != null)
		{
			Vector3 pickedObjectPosition = _grabbedItem.GlobalPosition;
			Vector3 handPosition = _grabPosition.GlobalPosition;

			Vector3 targetLinearVelocity = (handPosition - pickedObjectPosition) * 5.0f;
			targetLinearVelocity.Y *= 0.5f;
			_grabbedItem.LinearVelocity = targetLinearVelocity;
		}
	}

	private void CheckInteraction(Node3D targetItem)
	{
		if (_focusedItem != null)
		{
			return;
		}

		InteractionType t = CheckCanFocus(targetItem);

		if (t != _interactionState && t != InteractionType.None)
		{
			_focusedItem = targetItem;
			_interactionState = t;
			EmitSignal(SignalName.InteractionStateChanged, (int)t);
			EmitSignal(SignalName.Focused, targetItem);
		}
		else if (t != _interactionState && t == InteractionType.None)
		{
			_focusedItem = null;
			_interactionState = t;
			EmitSignal(SignalName.InteractionStateChanged, (int)t);
			EmitSignal(SignalName.LostFocus, targetItem);
		}
	}

	private InteractionType CheckCanFocus(Node3D targetItem)
	{
		if (targetItem.HasNode("Equipment"))
		{
			_canInteract = true;
			return InteractionType.Equip;
		}
		else if (targetItem.HasNode("Interaction"))
		{
			_canInteract = true;
			return InteractionType.Use;
		}
		else if (targetItem is RigidBody3D)
		{
			_canInteract = true;
			return InteractionType.Grab;
		}
		else
		{
			return InteractionType.None;
		}
	}

	private void CheckLoseFocus(Node3D targetItem)
	{
		if (_focusedItem == targetItem)
		{
			LoseFocus();
		}
	}

	private void LoseFocus()
	{
		EmitSignal(SignalName.LostFocus, _focusedItem);
		_focusedItem = null;
		_interactionState = InteractionType.None;
		EmitSignal(SignalName.InteractionStateChanged, (int)_interactionState);

	}

	private void Use(Node3D targetItem)
	{
		if (equipmentManager.EquippedItem != null)
		{
			equipmentManager.EquippedItem.GetNode<Interaction>("Interaction")?.Use(GetOwner<CharacterBase>());
		}
		else if (targetItem.GetNode("Interaction") is Interaction i)
		{
			i.Use(GetOwner<CharacterBase>());
		}
	}

	private bool Grab(RigidBody3D item)
	{
		if (_grabbedItem == null)
		{
			_grabbedItem = item;

			_grabJoint.NodeB = _grabbedItem.GetPath();
			return true;
		}
		else
		{
			_grabJoint.NodeB = null;
			_grabbedItem = null;
		}

		return false;
	}

	private void Drop(float force = 1.0f)
	{
		if (_grabbedItem != null)
		{
			_grabbedItem = null;
			_grabJoint.NodeB = null;
			_focusedItem = null;
		}
	}

	private void Equip()
	{
		Debug.Assert(false, "Not yet implemented!");
	}

	private void Take()
	{
		Debug.Assert(false, "Not yet implemented!");
	}
}
