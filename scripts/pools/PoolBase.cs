using Godot;
using System;

namespace ProjectMina;

[GlobalClass]
public partial class PoolBase : Node
{
	protected int PoolSize = 32;
	protected bool EnableDebug = false;
	protected bool AllowOverflow = true;
	
	public override void _Ready()
	{
		if (PoolSize < 1)
		{
			SetProcess(false);
			SetPhysicsProcess(false);
			return;
		}
	}

	public override void _Process(double delta)
	{
	}
}
