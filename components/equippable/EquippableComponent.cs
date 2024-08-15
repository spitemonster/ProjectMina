using GDebugPanelGodot.Extensions;
using Godot;
using Godot.Collections;
namespace ProjectMina;
public enum EEquipmentType
{
	Weapon,
	Tool
};

[GlobalClass]
public partial class EquippableComponent : InteractableComponent
{
	
	[Signal] public delegate void EquippedEventHandler(CharacterBase character, Node3D slot);
	[Signal] public delegate void UnequippedEventHandler(CharacterBase previousCharacter, Node3D slot);
	[Signal] public delegate void UsedEventHandler(CharacterBase character);
	
	public EEquipmentType EquipmentType { get; protected set; } = EEquipmentType.Tool;
	public Node3D EquipmentSlot;
	
	public bool CanEquip { get; private set; } = true;
	public bool CanUse { get; private set; } = true;

	protected RigidBody3D Body;
	public CharacterBase Wielder { get; protected set; }
	
	private uint _defaultCollisionLayer;
	private uint _defaultCollisionMask;
	private uint _defaultVisibilityLayer;

	public virtual void Use()
	{
		
	}

	public virtual void EndUse()
	{
		
	}

	public virtual void DisableUse()
	{
		CanUse = false;
	}

	public virtual void EnableUse()
	{
		CanUse = true;
	}

	public virtual CharacterBase EquippedBy()
	{
		return Wielder;
	}

	public virtual void Equip(CharacterBase equippingCharacter, Node3D slot)
	{
		Wielder = equippingCharacter;
		_DisableEquip();
		Body.Freeze = true;
		Body.CollisionLayer = 0;
		Body.CollisionMask = 0;
		
		Body.Reparent(slot);
		//
		// Body.RemoveParent();
		// slot.AddChild(Body);
		
		Body.GlobalTransform = slot.GlobalTransform;

		DebugDraw.Sphere(slot.GlobalTransform, .1f, Colors.Pink, 3f);

		if (Mesh != null)
		{
			Mesh.SetLayerMaskValue(1, false);
			Mesh.SetLayerMaskValue(2, true);	
		}
		
		EmitSignal(SignalName.Equipped, equippingCharacter, slot);
	}

	public virtual void Unequip(CharacterBase character, Node3D slot)
	{
		Wielder = null;
		_EnableEquip();
		
		Body.Freeze = false;
		Body.CollisionLayer = _defaultCollisionLayer;
		Body.CollisionMask = _defaultCollisionMask;
		Body.Reparent(Global.Data.CurrentLevel);

		if (Mesh != null)
		{
			Mesh.SetLayerMaskValue(1, true);
			Mesh.SetLayerMaskValue(2, false);
		}
		
		EmitSignal(SignalName.Unequipped, character, slot);
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
		Body = GetOwner<RigidBody3D>();
		
		if (Body == null)
		{
			GD.PushError("Equippable component parent: ", Owner.Name, " is not RigidBody3D.");
			_DisableEquip();
			return;
		}

		if (Mesh == null)
		{
			return;
		}

		_defaultCollisionLayer = Body.CollisionLayer;
		_defaultCollisionMask = Body.CollisionMask;
		_defaultVisibilityLayer = Mesh.Layers;
		
		base._Ready();
	}
}
