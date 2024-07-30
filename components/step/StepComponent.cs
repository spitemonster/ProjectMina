using Godot;

namespace ProjectMina;

public struct StepResult
{
	public bool IsStep = false;
	public Vector3 PositionDiff = Vector3.Zero;
	public Vector3 Normal = Vector3.Zero;
	public bool IsStepUp = false;

	public StepResult()
	{
		
	}
}

public partial class StepComponent : ComponentBase
{
	public StepResult TestStep(double delta, bool isJumping)
	{
		
		return new();
	}
}
