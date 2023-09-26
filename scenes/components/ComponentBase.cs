using Godot;

namespace ProjectMina;
/// <summary>
///  base class for all components, provides a number of properties for child components to inherit but should not be instantiated on its own.
/// </summary>
[Tool]
[GlobalClass]
public abstract partial class ComponentBase : Node
{
	[ExportGroup("ComponentBase")]
	[Export] protected bool _debug = false;
	[Export] protected bool _active = true;
	[Export] protected bool _shouldProcess = true;
	[Export] protected bool _shouldPhysicsProcess = true;

	public override void _Ready()
	{
		if (Engine.IsEditorHint())
		{
			return;
		}

		base._Ready();

		SetProcess(_shouldProcess);
		SetPhysicsProcess(_shouldPhysicsProcess);
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
		base._Process(delta);

		// don't run in the editor
		if (Engine.IsEditorHint())
		{
			return;
		}
	}
}
