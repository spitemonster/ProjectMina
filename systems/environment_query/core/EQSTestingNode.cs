using Godot;
using System;
using System.Collections.Generic;
using Godot.Collections;

namespace ProjectMina.EQS;
[Tool]
public partial class EQSTestingNode : CharacterBase
{
	private EnvironmentQuery _query;
	private Context _context;
	private Array<Test> _tests;

	private AIControllerComponent _controller;

	private Array<QueryPoint> _points;
	
	public override void _Ready()
	{
		// _query = GetNode<Query>("%Query");
		_context = GetNode<Context>($"%Context");
		_controller = new();
		AddChild(_controller);
		_controller.Possess(this);

		_InitPoints();
	}

	private async void _InitPoints()
	{
		_points = await _context.GeneratePoints(_controller);
	}

	public override void _PhysicsProcess(double delta)
	{
		foreach (var point in _points)
		{
			Color color = (point.TotalScore / point.TestCount) switch
			{
				> .99f => Colors.Lime,
				> .5f => Colors.SteelBlue,
				> 0 => Colors.Red,
				_ => Colors.Black
			};

			DebugDraw.Sphere(point.GlobalPosition, 1, color, 5f);
		}
	}
}
