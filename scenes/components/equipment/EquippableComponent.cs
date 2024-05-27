using Godot;
using Godot.Collections;
namespace ProjectMina;

public partial class EquippableComponent : InteractableComponent
{
	public enum EquipmentType
	{
		Weapon,
		Tool
	};
	
	[Signal] public delegate void EquippedEventHandler(CharacterBase wielder);
	[Signal] public delegate void UnequippedEventHandler(CharacterBase previousWielder);
	
	[Export] public EquipmentType Type = EquipmentType.Tool;
	
	public Node3D EquipmentSlot;
	
	public bool CanEquip = true;
	public bool CanUse = true;
	
	/// <summary>
	/// for weapons these are attack animations; the more you have the more varied your attacks are. this is more relevant to melee weapons.
	/// for tools this would be your tool use animations
	/// </summary>
	[Export] protected AnimationLibrary FirstPersonUseAnimationLibrary;
	/// <summary>
	///  third person/ai
	/// </summary>
	[Export] protected AnimationLibrary UseAnimationLibrary;
	/// <summary>
	/// we should expect the first person equipped animation library to include these four animations:
	/// 1. hand_pose
	/// 2. idle
	/// 3. walk
	/// 4. run
	/// </summary>
	[Export] protected AnimationLibrary FirstPersonEquippedAnimationLibrary;
	[Export] protected AnimationLibrary EquippedAnimationLibrary;

	private CharacterBase _wielder;
	private RigidBody3D _parent;

	public void Equip(CharacterBase character, Node3D slot)
	{
		if (_wielder != null)
		{
			return;
		}

		EquipmentSlot = slot;
		_wielder = character;
		// _parent.Reparent(EquipmentSlot);
		// _parent.TopLevel = false;
		// _parent.Transform = new();
		// _parent.Sleeping = true;
		// _parent.Freeze = true;
		GD.Print(character.Name, " equipped: ", GetOwner<Node>().Name);
		EmitSignal(SignalName.Equipped, _wielder);
		
	}

	public void Unequip()
	{
		_wielder = null;
		EquipmentSlot = null;
	}

	public AnimationLibrary GetUseAnimations(bool firstPerson = false)
	{
		return firstPerson ? FirstPersonUseAnimationLibrary : UseAnimationLibrary;
	}

	public AnimationLibrary GetEquippedAnimations(bool firstPerson = false)
	{
		return firstPerson ? FirstPersonEquippedAnimationLibrary : EquippedAnimationLibrary;
	}

	public AnimationLibrary GetFirstPersonUseAnimations()
	{
		return FirstPersonUseAnimationLibrary;
	}

	private void _EnableEquip()
	{
		CanEquip = true;
	}

	private void _DisableEquip()
	{
		CanEquip = false;
	}

	public override void _Ready()
	{
		base._Ready();

		if (GetOwner<RigidBody3D>() is RigidBody3D r)
		{
			_parent = r;
		}
		else
		{
			GD.PushError("EquippableComponent attached to object that is not RigidBody3D.");
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (EquipmentSlot != null)
		{
			
			var currentPosition = _parent.GlobalPosition;
			var targetPosition = EquipmentSlot.GlobalPosition;
			var targetLinearVelocity = (targetPosition - currentPosition) * 100.0f;
			
			_parent.LinearVelocity = targetLinearVelocity;
			
			// _parent.GlobalTransform = EquipmentSlot.GlobalTransform;
		}
	}
}
