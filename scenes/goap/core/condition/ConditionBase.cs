using Godot;

namespace ProjectMina.Goap;

[GlobalClass]
public partial class ConditionBase : GodotObject
{
    public StringName Property;
    public EComparison Comparison;
    public int DesiredValue;
}
