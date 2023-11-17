using Godot;

public partial class Viewmodel : Node3D
{
	[Export] public SkeletonIK3D LeftArmIK { get; protected set; }
	[Export] public SkeletonIK3D RightArmIK { get; protected set; }
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Print("viewmodel ready");
		LeftArmIK?.Start();
		RightArmIK?.Start();
	}
}
