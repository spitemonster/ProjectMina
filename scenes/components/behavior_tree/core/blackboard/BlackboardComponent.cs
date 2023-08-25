using Godot;
using Godot.Collections;

namespace ProjectMina;

[Tool]
[GlobalClass, Icon("res://scenes/components/behavior_tree/core/icons/board-icon.svg")]
public partial class BlackboardComponent : Node
{
	[Signal] public delegate void ValueChangedEventHandler(string key, Variant newValue);
	[Export] public Resource Blackboard;

	public Array<string> Keys = new();

	private Dictionary<string, Variant> _blackboard = new();

	public override void _EnterTree()
	{
		if (Blackboard != null && Blackboard is BlackboardAsset bb && bb.Entries.Count > 0)
		{
			_blackboard = bb.Entries.Duplicate();
		}
	}

	public bool ValueEqual(string key, Variant compare)
	{
		Variant v = GetValue(key);

		if ((bool)v == false || GetValueType(key) != compare.GetType() || !v.Equals(compare))
		{
			return false;
		}

		return true;
	}

	public bool TypeEqual(string key, Variant compare) {
		Variant v = GetValue(key);
		
		return !((bool)v == false || GetValueType(key) != compare.GetType());
	}

	public bool HasValue(string key)
	{
		return _blackboard.ContainsKey(key);
	}

	public Variant GetValue(string key)
	{
		if (!HasValue(key))
		{
			return false;
		}

		return _blackboard[key];
	}

	public System.Type GetValueType(string key)
	{
		if (!HasValue(key))
		{
			return null;
		}

		return GetValue(key).GetType();
	}

	public bool SetValue(string key, Variant value)
	{
		// ensure our data contains the correct key, the types are the same and the value currently there is not the same as what is being submitted
		if (!HasValue(key) || _blackboard[key].GetType() != value.GetType() || _blackboard[key].Equals(value))
		{
			return false;
		}

		_blackboard[key] = value;
		EmitSignal(SignalName.ValueChanged, key, value);
		return true;
	}

	public bool EraseValue(string key)
	{
		if (!HasValue(key))
		{
			return false;
		}

		_blackboard[key] = default;
		EmitSignal(SignalName.ValueChanged, key, _blackboard[key]);
		return true;
	}

	public override string[] _GetConfigurationWarnings()
	{
		Array<string> warnings = new();

		foreach (var key in _blackboard.Keys)
		{
			if (key is not string)
			{
				warnings.Add("Blackboard keys must be of type string.");
				break;
			}
		}

		string[] baseWarnings = base._GetConfigurationWarnings();
		if (baseWarnings != null && baseWarnings.Length > 0)
		{
			warnings.AddRange(baseWarnings);
		}

		string[] errs = new string[warnings.Count];

		for (int i = 0; i < warnings.Count; i++)
		{
			errs.SetValue(warnings[i], i);
		}

		return errs;
	}
}