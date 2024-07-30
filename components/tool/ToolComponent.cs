using Godot;

namespace ProjectMina;
[GlobalClass]
public partial class ToolComponent : EquippableComponent
{
	public override void _Ready()
	{
		base._Ready();
		
		EquipmentType = EEquipmentType.Tool;
	}

	public override void _Process(double delta)
	{
	}
}
