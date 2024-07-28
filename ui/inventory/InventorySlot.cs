using Godot;
using System;

[Tool]
[GlobalClass]
public partial class InventorySlot : Control
{
	[Signal] public delegate void SlotEnteredEventHandler(int slotId);
	[Signal] public delegate void SlotExitedEventHandler(int slotId);
	
	public int SlotID;
	
	public override void _Ready()
	{
		MouseEntered += () =>
		{
			EmitSignal(SignalName.SlotEntered, SlotID);
		};

		MouseExited += () =>
		{
			EmitSignal(SignalName.SlotExited, SlotID);
		};
	}
}
