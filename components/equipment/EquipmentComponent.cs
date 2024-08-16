using Godot;

namespace ProjectMina;

/// <summary>
///  this component attaches to a characterbase and manages its equipment, that is the items it has in its left and right hand slots
///  as opposed to its inventory which is items in its possession but ont actively equipped 
/// </summary>

[GlobalClass]
public partial class EquipmentComponent : ComponentBase
{
    [Signal] public delegate void WeaponEquippedEventHandler(WeaponComponent weapon);
	[Signal] public delegate void ToolEquippedEventHandler(EquippableComponent tool);
	[Signal] public delegate void WeaponUnequippedEventHandler();
	[Signal] public delegate void ToolUnequippedEventHandler();

    [Signal] public delegate void RangedWeaponFiredEventHandler(RangedWeaponComponent rangedWeapon);

    [Export] public Node3D WeaponSlot { get; protected set; }
    public WeaponComponent EquippedWeapon = null;
    public bool IsWeaponEquipped => EquippedWeapon != null;
    
    public bool CanEquipFocusedObject { get; private set; }
    private CharacterBase _owner;
    private PlayerCharacter _player;

    public void FireRangedWeapon()
    {
        if (EquippedWeapon is RangedWeaponComponent r)
        {
            r.PullTrigger();
        }
    }

    public void DisableWeapon()
    {
        EquippedWeapon?.DisableUse();
    }

    public void EnableWeapon()
    {
        EquippedWeapon?.EnableUse();
    }
    
    public override void _Ready()
    {
        if (!Active) return;
        
        base._Ready();

        _owner = GetOwner<CharacterBase>();

        if (_owner == null)
        {
            GD.PushError("Owner is not character base!!!");
        }
    }

    public static bool CanEquip(RigidBody3D item)
    {
        return item.HasNode("Tool") || item.HasNode("Weapon");
    }

    public void Equip(RigidBody3D item)
    {
        if (item.GetNodeOrNull<ToolComponent>("Tool") is { } t)
        {
            _EquipTool(t);
        } else if (item.GetNodeOrNull<WeaponComponent>("Weapon") is { } w)
        {
            _EquipWeapon(w);
        }
    }

    private void _EquipTool(ToolComponent t)
    {
        
    }

    // TODO: find a more elegant way to do this
    private void _EquipWeapon(WeaponComponent weapon)
    {
        if (EquippedWeapon != null)
        {
            return;
        }

        EquippedWeapon = weapon;
        EmitSignal(SignalName.WeaponEquipped, EquippedWeapon);

        if (EquippedWeapon is RangedWeaponComponent r)
        {
            r.Fired += WeaponFired;
        }
    }

    public void AttachWeapon()
    {
        EquippedWeapon.Equip(_owner, WeaponSlot);
    }

    public void DropWeapon()
    {
        EquippedWeapon?.Unequip(_owner, WeaponSlot);
		EmitSignal(SignalName.WeaponUnequipped, EquippedWeapon);
        
        if (EquippedWeapon is RangedWeaponComponent r)
        {
            r.Fired -= WeaponFired;
        }
        
        EquippedWeapon = null;
    }

    public void WeaponFired(int remainingAmmoInClip, int remainingAmmoInReserve)
    {
        EmitSignal(SignalName.RangedWeaponFired, (RangedWeaponComponent)EquippedWeapon);
    }
    
    public void UseWeapon()
    {
        if (!IsWeaponEquipped) return;
        if (EquippedWeapon is RangedWeaponComponent r)
        {
            r.PullTrigger();
        }
        else
        {
            EquippedWeapon.Use();
        }
    }
	
    public void EndUseWeapon()
    {
        if (!IsWeaponEquipped) return;
		
        if (EquippedWeapon is RangedWeaponComponent r)
        {
            r.ReleaseTrigger();
        }
        else
        {
            EquippedWeapon.EndUse();
        }
    }
    
    public void ReloadRangedWeapon()
    {
        if (IsWeaponEquipped && EquippedWeapon is RangedWeaponComponent r)
        {
            r.Reload();
        }
    }
}
