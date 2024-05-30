using Godot;
using Godot.Collections;
using System.Diagnostics;
namespace ProjectMina;

[GlobalClass]
public partial class SearchComponent : ComponentBase
{
	[Export] public Area3D SearchArea { get; protected set; }

	public override void _Ready()
	{
		if (EnableDebug)
		{
			System.Diagnostics.Debug.Assert(SearchArea != null, "No search area");
		}
	}

	public Array<Node3D> GetAllObjectsInSearchArea()
	{
		return SearchArea.GetOverlappingBodies();
	}

	public Array<Node3D> GetInteractablesInArea()
	{
		Array<Node3D> interactables = new();

		foreach (Node3D body in GetAllObjectsInSearchArea())
		{
			if (body.HasNode("Interaction"))
			{
				interactables.Add(body);
			}
		}

		return interactables;
	}

	// public Array<Equipment> GetWeaponsInRadius()
	// {
	// 	// Array<Equipment> weapons = new();
	// 	//
	// 	// foreach (Node3D body in GetAllObjectsInSearchArea())
	// 	// {
	// 	// 	if (body.HasNode("Equipment"))
	// 	// 	{
	// 	// 		Equipment e = body.GetNode<Equipment>("Equipment");
	// 	//
	// 	// 		if (e.Type == Equipment.EquipmentType.Weapon)
	// 	// 		{
	// 	// 			weapons.Add(e);
	// 	// 		}
	// 	// 	}
	// 	// }
	// 	//
	// 	// return weapons;
	// }

}
