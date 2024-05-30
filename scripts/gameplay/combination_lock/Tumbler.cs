using Godot;
using System;

namespace ProjectMina;

[GlobalClass]
public partial class Tumbler : Node3D
{
	[Signal] public delegate void SettingChangedEventHandler(int newSetting);
	[Signal] public delegate void TumblerTurnedEventHandler();
	[Export] private UsableComponent _usableComponent;
	[Export] private int _numSides = 10;
	
	private Tween _tween;
	public override void _Ready()
	{
		if (_usableComponent != null)
		{
			_usableComponent.InteractionStarted += _AddRotation;
		}
		
		_tween = GetTree().CreateTween();
		_tween.Stop();
	}

	// converts the tumbler's rotation to a digit between 0-9
	public int GetSetting()
	{
		var rotationDegrees = Mathf.PosMod(RotationDegrees.X, -360.0f);
		int digit = (10 + Mathf.FloorToInt(rotationDegrees / (-360.0f / _numSides))) % 10;
		return digit;
	}

	public string GetSettingAsString()
	{
		return GetSetting().ToString();
	}

	private void _AddRotation(CharacterBase character)
	{
		
		Tween t = GetTree().CreateTween();
		Vector3 targetRotation = RotationDegrees + new Vector3(-36, 0, 0);
		PropertyTweener tw = t.TweenProperty(this, "rotation_degrees", targetRotation, .2);
		tw.Finished += () =>
		{
			_usableComponent.CompleteInteraction(character); 
			EmitSignal(SignalName.TumblerTurned);
		};
	}
	
}
