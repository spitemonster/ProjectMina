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

    [Export] protected WeaponData Data;
    public EWeaponType WeaponType => Data.WeaponType;
    
    public override void _Ready()
    {
        if (Data == null)
        {
            GD.PushError("no weapon data");
        }
        
        EquipmentType = EEquipmentType.Weapon;
        base._Ready();
    }
    
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
}
