using Godot;

namespace ProjectMina;

public enum ESenseType : uint
{
	None,
	SightStatic,
	SightDynamic,
	HearingStatic,
	HearingDynamic
}

public enum ESensePriority : uint
{
	None,
	Low,
	Medium,
	High
}

public partial class SenseContext : RefCounted
{
	public ESenseType SenseType;
	public Node3D Owner;
	public Vector3 Position;
}

[GlobalClass]
public partial class AISenseComponent : Area3D
{
	[Signal] public delegate void SenseTriggeredEventHandler(SenseContext context);
}
