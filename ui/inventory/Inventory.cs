using Godot;
using Godot.Collections;

[Tool]
public partial class Inventory : Control
{
	[Export] protected int SlotCount = 80;
	[Export] protected PackedScene SlotScene;
	protected GridContainer InventoryGrid;

	protected Dictionary<int, InventorySlot> Slots = new();
	public override void _EnterTree()
	{
		if (SlotScene == null)
		{
			return;
		}

		var scene = ResourceLoader.Load<PackedScene>(SlotScene.ResourcePath);
		InventoryGrid = GetNode<GridContainer>("%InventoryGrid");
		for (int i = 0; i < SlotCount; i++)
		{
			var slot = scene.Instantiate();

			if (slot is not InventorySlot s)
			{
				return;
			}

			s.SlotID = i;
			Slots.Add(i, s);
			InventoryGrid.AddChild(s);
		}
	}

	public void _OnSlotEntered(int slotID)
	{
		
	} 

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
