using Godot;

namespace ProjectMina;

/// <summary>
///  this component attaches to a characterbase and manages its equipment, that is the items it has in its left and right hand slots
///  as opposed to its inventory which is items in its possession but ont actively equipped 
/// </summary>

public partial class EquipmentComponent : ComponentBase
{
	[Signal] public delegate void WeaponEquippedEventHandler(WeaponComponent weapon);
	[Signal] public delegate void ToolEquippedEventHandler(EquippableComponent tool);
	[Signal] public delegate void WeaponUnequippedEventHandler();
	[Signal] public delegate void ToolUnequippedEventHandler();
	[Export] public Node3D LeftHandSlot { get; protected set; }
	[Export] public Node3D RightHandSlot { get; protected set; }
	
	/// <summary>
	/// if this equipment component is attached to a first person character, such as the player character
	/// we should request first person-focused animations
	/// for most characters this is not the case
	/// </summary>
	[Export] private bool _requestFirstPersonAnims = false;

	private ToolComponent _equippedTool;
	private WeaponComponent _equippedWeapon;

	private CharacterBase _owner;

	public bool IsRangedWeaponEquipped()
	{
		return IsWeaponEquipped() && GetWeapon().WeaponType == EWeaponType.Ranged;
	}
	
	public bool IsMeleeWeaponEquipped()
	{
		return IsWeaponEquipped() && GetWeapon().WeaponType == EWeaponType.Melee;
	}

	public WeaponComponent GetWeapon()
	{
		return _equippedWeapon;
	}

	public bool IsWeaponEquipped()
	{
		return _equippedWeapon != null;
	}

	public bool IsToolEquipped()
	{
		return _equippedTool != null;
	}

	public EquippableComponent CanEquip(Node node)
	{
		GD.Print("testing can equip against: ", node.Name);
		if (node is EquippableComponent e)
		{
			return e;
		}
		
		if (node.GetNodeOrNull<ToolComponent>("%Tool") is { } t)
		{
			return t;
		}
		
		if (node.GetNodeOrNull<RangedWeaponComponent>("%RangedWeapon") is { } w)
		{
			return w;
		}
		
		if (node.GetNodeOrNull<RangedWeaponComponent>("%MeleeWeapon") is { } m)
		{
			return m;
		}
		
		return null;
	}

	public void Equip(EquippableComponent equippable)
	{
		if (EnableDebug)
		{
		}
		switch (equippable.EquipmentType)
		{
			case EEquipmentType.Tool:
				if (equippable is ToolComponent t)
				{
					EquipTool(t);	
				}
				else
				{
					GD.PushError("non-tool equippable component set to type tool");
				}
				
				break;
			case EEquipmentType.Weapon:
				if (equippable is WeaponComponent w)
				{
					EquipWeapon(w);	
				}
				else
				{
					GD.PushError("non-weapon equippable component set to type weapon");
				}
				break;
		}
	}
	
	public void EquipTool(ToolComponent tool)
	{
		if (EnableDebug)
		{
		}
		
		if (_equippedTool != null)
		{
			DropTool();
		}

		_equippedTool = tool;
		_equippedTool.Equip(_owner, LeftHandSlot);
		EmitSignal(SignalName.ToolEquipped, _equippedTool);
	}

	public void DropTool()
	{
		if (EnableDebug)
		{
		}

		_equippedTool?.Unequip(_owner, LeftHandSlot);
		EmitSignal(SignalName.ToolUnequipped, _equippedTool);
	}

	public void EquipWeapon(WeaponComponent weapon)
	{
		_equippedWeapon = weapon;
		weapon.Equip(_owner, RightHandSlot);
		EmitSignal(SignalName.WeaponEquipped, _equippedWeapon);
	}

	public void UseWeapon()
	{
		if (IsWeaponEquipped())
		{
			var w = GetWeapon();

			if (w is RangedWeaponComponent r)
			{
				r.PullTrigger();
			}
			else
			{
				w.Use();
			}
		}
	}
	
	public void EndUseWeapon()
	{
		if (IsWeaponEquipped())
		{
			var w = GetWeapon();

			if (w is RangedWeaponComponent r)
			{
				r.ReleaseTrigger();
			}
			else
			{
				w.EndUse();
			}
		}
	}

	public void ReloadWeapon()
	{
		if (IsWeaponEquipped() && GetWeapon() is RangedWeaponComponent r)
		{
			r.Reload();
		}
	}

	public void DropWeapon()
	{
		_equippedWeapon?.Unequip(_owner, RightHandSlot);
		EmitSignal(SignalName.WeaponUnequipped, _equippedWeapon);
	}
	
	public override void _Ready()
	{
		_owner = GetOwner<CharacterBase>();
		
		GD.Print("right hand slot: ", RightHandSlot);
	}

	public override void _Process(double delta)
	{
	}
}
