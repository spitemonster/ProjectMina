using Godot;
using System;

namespace ProjectMina;

[GlobalClass]
public partial class ComponentBase3D : Node3D
{
	[ExportGroup("ComponentBase")]
	[Export] protected bool Debug = false;
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
