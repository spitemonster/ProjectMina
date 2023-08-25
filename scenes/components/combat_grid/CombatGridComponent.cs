using Godot;

namespace ProjectMina;
[Tool]
[GlobalClass]
public partial class CombatGridComponent : Node3D
{
	[Export]
	public Godot.Collections.Array<CombatGridPoint> GridPoints;

	private PlayerCharacter _owner;

	public CombatGridPoint GetPoint()
	{
		CombatGridPoint p = null;
		foreach (CombatGridPoint point in GridPoints)
		{
			if (!point.Occupied)
			{
				p = point;
			}
		}

		return p;
	}

	public override void _Ready()
	{
		_owner = GetOwner<PlayerCharacter>();
		GridPoints = new();

		CallDeferred("CountPoints");
	}

	public override void _Process(double delta)
	{
	}

	private void CountPoints()
	{
		Godot.Collections.Array<Node> children = GetChildren();

		foreach (Node node in children)
		{
			if (node is Node3D && node is CombatGridPoint p)
			{
				GridPoints.Add(p);
			}
		}

	}
}
