using Godot;
using System;

namespace ProjectMina;

[GlobalClass]
public partial class EnvironmentManager : ManagerBase
{
	[Export] protected int DayLength = 24;
	[Export] protected bool Enabled = true;
	private DirectionalLight3D _sun;

	private float _midnightSunRotation = 30f;
	private Timer _dayTimer;

	public override void _Ready()
	{
		base._Ready();

		_dayTimer = new()
		{
			WaitTime = DayLength * 60,
			Autostart = false,
			OneShot = false
		};
		
		AddChild(_dayTimer);

		_dayTimer.Timeout += _EndDay;

		CallDeferred("_Setup");
	}

	private void _EndDay()
	{
		
	}

	private void _Setup()
	{
		_sun = Global.Data.CurrentLevel.GetNode<DirectionalLight3D>("%Sun");

		if (_sun != null && Enabled)
		{
			_dayTimer.Start();
		}
	}

	public override void _Process(double delta)
	{
		var timeRatio = (_dayTimer.GetWaitTime() - _dayTimer.GetTimeLeft()) / _dayTimer.GetWaitTime();

		var rot = (float)Mathf.Lerp(0, -359, timeRatio);
		
		if (Enabled && _sun != null && !_dayTimer.IsStopped())
		{
			var sunRot = _sun.GlobalRotation;
			sunRot.X = Mathf.DegToRad(rot);
			_sun.GlobalRotation = sunRot;
		}

		if (!Enabled && !_dayTimer.IsStopped())
		{
			_dayTimer.Stop();
		}
	}
}