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
	
	[Export] protected Array<StringName> FirstPersonUseAnimations = new();
	[Export] protected AnimationLibrary FirstPersonUseAnimationLibrary;
	[Export] protected Array<StringName> FirstPersonEquipAnimations = new();

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
		GD.Print(character.Name, " equipped: ", GetOwner<Node>().Name);
		EmitSignal(SignalName.Equipped, _wielder);
	}

	public void Unequip()
	{
		_wielder = null;
		EquipmentSlot = null;
	}

	public AnimationLibrary GetFirstPersonAnimations()
	{
		return FirstPersonUseAnimationLibrary;
	}

	public StringName GetUseAnim(bool firstPerson = false)
	{
		if (firstPerson)
		{
			return FirstPersonUseAnimations.PickRandom();
		}

		return "";
	}

	public StringName GetEquipAnim(bool firstPerson = false)
	{
		if (firstPerson)
		{
			return FirstPersonEquipAnimations.PickRandom();
		}

		return "";
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
			
			// var currentPosition = _parent.GlobalPosition;
			// var targetPosition = EquipmentSlot.GlobalPosition;
			// var targetLinearVelocity = (targetPosition - currentPosition) * 10.0f;
			//
			// _parent.LinearVelocity = targetLinearVelocity;
			
			_parent.GlobalTransform = EquipmentSlot.GlobalTransform;
		}
	}
}
