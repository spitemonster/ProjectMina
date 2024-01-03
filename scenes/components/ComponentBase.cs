using Godot;

namespace ProjectMina;
/// <summary>
///  base class for all components, provides a number of properties for child components to inherit but should not be instantiated on its own.
/// </summary>
[GlobalClass]
public abstract partial class ComponentBase : Node
{
	[ExportGroup("ComponentBase")]
	[Export] protected bool EnableDebug = false;
	[Export] protected bool Active = true;
	[Export] protected bool ShouldProcess = true;
	[Export] protected bool ShouldPhysicsProcess = true;

	public override void _Ready()
	{
		if (Engine.IsEditorHint())
		{
			return;
		}

		base._Ready();

		SetProcess(ShouldProcess);
		SetPhysicsProcess(ShouldPhysicsProcess);
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		// don't run in the editor
		if (Engine.IsEditorHint())
		{
			return;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		// don't run in the editor
		if (Engine.IsEditorHint())
		{
			return;
		}
	}
}
