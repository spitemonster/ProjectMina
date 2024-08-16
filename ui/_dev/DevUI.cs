using Godot;
using Godot.Collections;

namespace ProjectMina;
public partial class DevUI : Control
{
	[Export] protected PackedScene DevMonitorGroupScene;
	[Export] protected PackedScene DevMonitorScene;

	private Label _FPSLabel;
	private Label _maxFPSLabel;
	private Label _minFPSLabel;
	private Label _frameTimeLabel;
	private VBoxContainer _monitorContainer;
	private VBoxContainer _notificationQueue;
	private DevMonitorGroup _globalDevMonitorGroup;
	private DevMonitor _fpsMonitor;
	private DevMonitor _maxFpsMonitor;
	private DevMonitor _minFpsMonitor;
	private DevMonitor _frameTimeMonitor;
	
	private double _minFps = 120.0;
	private double _maxFps = 0.0;
	private bool _minFPSTimeoutEnded = false;
	private PackedScene ui;
	private int _lastuSec = 0;

	private Dictionary<StringName, DevMonitorGroup> _monitorGroups = new();
	private Timer _minFpsTimer;

	public override void _Ready()
	{
		base._Ready();
		
		_monitorContainer = GetNodeOrNull<VBoxContainer>("%MonitorContainer");
		
		_globalDevMonitorGroup = GetNodeOrNull<DevMonitorGroup>("%GlobalMonitorGroup");
		_fpsMonitor = GetNodeOrNull<DevMonitor>("%FPSMonitor");
		_maxFpsMonitor = GetNodeOrNull<DevMonitor>("%MaxFPSMonitor");
		_minFpsMonitor = GetNodeOrNull<DevMonitor>("%MinFPSMonitor");
		_frameTimeMonitor = GetNodeOrNull<DevMonitor>("%FrameTimeMonitor");
		_notificationQueue = GetNodeOrNull<VBoxContainer>("%NotificationQueue");

		_minFpsTimer = new()
		{
			OneShot = false,
			WaitTime = 5f
		};

		_minFpsTimer.Timeout += () =>
		{
			_minFps = 120.0f;
			_minFpsMonitor.SetValue("120");
		};
		
		AddChild(_minFpsTimer);
		_minFpsTimer.Start();
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		double currentFps = Engine.GetFramesPerSecond();

		_fpsMonitor.SetValue(currentFps.ToString());

		var frameTime = System.Math.Round(1000.00 / currentFps, 2);
		_frameTimeMonitor.SetValue(frameTime.ToString());

		if (currentFps < _minFps)
		{
			_minFps = currentFps;
			_minFpsMonitor.SetValue(_minFps.ToString());
			_minFpsTimer.Stop();
			_minFpsTimer.Start();
		}

		if (currentFps > _maxFps)
		{
			_maxFps = currentFps;
			_maxFpsMonitor.SetValue(_maxFps.ToString());
		}
	}

	public DevMonitorGroup AddDevMonitorGroup(StringName groupName)
	{
		DevMonitorGroup newGroup = DevMonitorGroupScene.Instantiate<DevMonitorGroup>();

		if (newGroup == null)
		{
			GD.PushError("there was an error creating the monitor group: ", groupName);
			return null;
		}
		
		_monitorContainer.AddChild(newGroup);
		newGroup.SetTitle(groupName);
		
		_monitorGroups.Add(groupName, newGroup);
		return newGroup;
	}

	public DevMonitorGroup GetDevMonitorGroup(StringName groupName)
	{
		if (!_monitorGroups.ContainsKey(groupName))
		{
			return AddDevMonitorGroup(groupName);
		}

		return _monitorGroups[groupName];
	}
	
	public DevMonitor AddDevMonitor(string label, Color valueColor, StringName group)
	{
		// LabelValueRow newMonitor = new(label, valueColor);
		DevMonitor newMonitor = DevMonitorScene.Instantiate<DevMonitor>();

		if (newMonitor == null)
		{
			GD.PushError("there was an error creating the monitor: ", label);
			return null;
		}

		if (group == null)
		{
			_globalDevMonitorGroup.AddChild(newMonitor);	
		}
		
		GetDevMonitorGroup(group).AddChild(newMonitor);
		
		newMonitor.SetLabel(label);
		newMonitor.SetValueColor(valueColor);
		return newMonitor;
	}

	public void PushDevNotification(string notification)
	{
		Growler msg = new()
		{
			Text = notification
		};

		_notificationQueue.AddChild(msg);
	}
}
