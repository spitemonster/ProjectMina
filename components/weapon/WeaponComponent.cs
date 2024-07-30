using Godot;
namespace ProjectMina;

public enum EWeaponType : int
{
    None,
    Melee,
    Ranged
};

[GlobalClass]
public partial class WeaponComponent : EquippableComponent
{

    public EWeaponType WeaponType = EWeaponType.None;
    
    
    
    protected virtual void Attack()
    {
        if (!CanUse)
        {
            return;
        }
    }
    
    protected virtual void EndAttack()
    {
        
    }
    
    public override void Use()
    {
        Attack();
    }

    public override void EndUse()
    {
        EndAttack();
    }

    public override void _Ready()
    {
        base._Ready();

        EquipmentType = EEquipmentType.Weapon;
    }
}
