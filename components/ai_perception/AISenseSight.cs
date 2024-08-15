using System.Diagnostics;
using System.Linq;
using Godot;
using Godot.Collections;

namespace ProjectMina;

[GlobalClass]
public partial class AISenseSight : AISenseComponent
{
	[Signal] public delegate void CharacterEnteredLineOfSightEventHandler(CharacterBase character);
	[Signal] public delegate void CharacterExitedLineOfSightEventHandler(CharacterBase character);
	[Signal] public delegate void NodeEnteredLineOfSightEventHandler(Node3D node);
	[Signal] public delegate void NodeExitedLineOfSightEventHandler(Node3D node);
	
	// [Export] protected int HighPriorityCastCount = 8;
	// [Export] protected float NoticeThreshold = 10f;
	// [Export] protected float SeenThreshold = 20f;

	[Export] protected bool EnableDebug = false;
	[Export] protected bool Enabled = true;

	// private Array<AIStimulusSight> _stimuli = new();
	private Array<Node3D> _nodesInSightArea = new();
	private Array<Node3D> _nodesInLineOfSight = new();

	private DevMonitor _devMonitor;
	private AICharacter _owner;
	
	public override void _Ready()
	{
		if (Enabled)
		{
			BodyEntered += _CheckOverlap;
			BodyExited += _CheckExitOverlap;

			CallDeferred("_InitDevMonitor");
			CallDeferred("_CheckInitialOverlaps");	
		}
		

		_owner = GetOwner<AICharacter>();
	}

	private void _InitDevMonitor()
	{
		_devMonitor = Dev.UI.AddDevMonitor("Visibility", Colors.DarkGoldenrod, "AI");
	}

	private void _CheckInitialOverlaps()
	{
		var bodies = GetOverlappingBodies();
		
		foreach (var body in bodies)
		{
			_CheckOverlap(body);	
		}
	}

	public override void _PhysicsProcess(double delta)
	{

		foreach (var node in _nodesInSightArea)
		{
			// los trace target changes depending on context
			var target = node;
			
			if (node is CharacterBase c)
			{
				target = c.Head;
			}
			
			var haveLineOfSight = _RunSightTrace(target);
			
			if (!haveLineOfSight)
			{
				_TryRemoveNodeLineOfSight(node);
				return;
			}
			
			_TryAddNodeLineOfSight(node);
		}
		// _TestStimulus(delta);

		// if (_testVal >= NoticeThreshold && !_stimulusNoticed)
		// {
		// 	var context = new StimulusContext()
		// 	{
		// 		Owner = (Node3D)_stimuli[_currentCastTarget].GetOwner(),
		// 		Stimulus = _stimuli[_currentCastTarget],
		// 		SenseType = ESenseType.SightDynamic
		// 	};
		//
		// 	EmitSignal(SignalName.StimulusNoticed, context);
		// 	_stimulusNoticed = true;
		// }
		//
		// if (_testVal >= SeenThreshold && !_stimulusSeen)
		// {
		// 	var context = new StimulusContext()
		// 	{
		// 		Owner = (Node3D)_stimuli[_currentCastTarget].GetOwner(),
		// 		Stimulus = _stimuli[_currentCastTarget],
		// 		SenseType = ESenseType.SightDynamic
		// 	};
		// 	
		// 	EmitSignal(SignalName.StimulusSeen, context);
		// 	_stimulusSeen = true;
		// }
		//
		// if (_currentCastTarget + 1 < _stimuli.Count)
		// {
		// 	_currentCastTarget++;
		// }
		// else
		// {
		// 	_currentCastTarget = 0;
		// }
		//
		// if (_currentTrace + 1 < HighPriorityCastCount)
		// {
		// 	_currentTrace++;
		// }
		// else
		// {
		// 	_currentTrace = 0;
		// }
		//
		// if (_testVal > 25f)
		// {
		// 	_testVal = 25f;
		// }
		//
		// if (_testVal > 0)
		// {
		// 	_testVal -= _reductionRate * (float)delta;
		// } else if (_testVal < 0)
		// {
		// 	_testVal = 0;
		// }
		//
		// if (_testVal < NoticeThreshold && _stimulusNoticed)
		// {
		// 	_stimulusNoticed = false;
		// }
		//
		// if (_testVal < SeenThreshold && _stimulusSeen)
		// {
		// 	_stimulusSeen = false;
		// }
		
		// _devMonitor.SetValue(_testVal.ToString());
	}
	
	private void _TryAddNodeLineOfSight(Node3D node)
	{
		if (_nodesInLineOfSight.Contains(node))
		{
			return;
		}
		
		_nodesInLineOfSight.Add(node);
		
		if (node is CharacterBase c) {
            EmitSignal(SignalName.CharacterEnteredLineOfSight, c);
        }

		EmitSignal(SignalName.NodeEnteredLineOfSight);
	}

	private void _TryRemoveNodeLineOfSight(Node3D node)
	{
		if (!_nodesInLineOfSight.Contains(node))
		{
			return;
		}

		_nodesInLineOfSight.Remove(node);
		
		if (node is CharacterBase c) {
			EmitSignal(SignalName.CharacterExitedLineOfSight, c);
		}

		EmitSignal(SignalName.NodeExitedLineOfSight);
	}

	private void _TestStimulus(double delta)
	{
		// if (_stimuli.Count < 1)
		// {
		// 	return;
		// }
		//
		// var currentTarget = _stimuli[_currentCastTarget];
		// var haveLineOfSightToTarget = _RunSightTrace(currentTarget);
		//
		// if (haveLineOfSightToTarget)
		// {
		// 	_testVal += (_accumulationRate * (float)delta);
		// }
	}

	private bool _RunSightTrace(Node3D target)
	{
		if (!Enabled)
		{
			return false;
		}
		
		if (target == null)
		{
			if (EnableDebug) {
            	GD.PushError("target is null.");
            }
			return false;
		}

		var dist = GlobalPosition.DistanceTo(target.GlobalPosition);
		var dir = GlobalPosition.DirectionTo(target.GlobalPosition);

		PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;

		PhysicsRayQueryParameters3D traceQuery = new()
		{
			From = GlobalPosition,
			To = GlobalPosition + dir * dist,
			Exclude = new() { GetRid(), _owner.GetRid() },
			CollideWithAreas = false,
			CollideWithBodies = true,
			CollisionMask = 0b00000000_00001000_00000000_00000011,
			
		};

		var res = spaceState.IntersectRay(traceQuery);

		if (!res.ContainsKey("collider"))
		{
			DebugDraw.Line(GlobalPosition, GlobalPosition + dir * dist, Colors.Green);
			return false;
		}

		var collider = (Node3D)res["collider"];

		if (EnableDebug) {
        	GD.Print("collider name: ", collider.Name);
        }

		if (collider == target || collider == target.GetOwner() || collider.GetOwner() == target.GetOwner())
		{
			// DebugDraw.Line(GlobalPosition, target.GlobalPosition, Colors.DarkGoldenrod);
			DebugDraw.Line(GlobalPosition, GlobalPosition + dir * dist, Colors.Red);
			DebugDraw.Sphere((Vector3)res["position"], 1, Colors.Red);
			return true;
		}
		
		return false;
	}

	private void _CheckOverlap(Node3D node)
	{
		if (EnableDebug) {
			GD.Print("checking overlap with node: ", node.Name);
		}
		
		if (node == Owner || _nodesInSightArea.Contains(node))
		{
			return;
		}
		
		_nodesInSightArea.Add(node);

		if (_RunSightTrace(node))
		{
			_TryAddNodeLineOfSight(node);
		}
	}

	private void _CheckExitOverlap(Node3D node)
	{
				
		if (EnableDebug) {
			GD.Print("checking exit overlap with node: ", node.Name);
		}
		
		if (node == Owner || !_nodesInSightArea.Contains(node))
		{
			return;
		}
		
		_TryRemoveNodeLineOfSight(node);

		_nodesInSightArea.Remove(node);
	}
}
