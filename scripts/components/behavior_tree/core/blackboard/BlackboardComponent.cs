using Godot;
using Godot.Collections;

namespace ProjectMina;

[Tool]
[GlobalClass]
public partial class BlackboardComponent : ComponentBase
{
	[Signal] public delegate void ValueChangedEventHandler(StringName key, Variant newValue);
	[Export] public Resource Blackboard;

	public Array<StringName> Keys = new();

	private Dictionary<StringName, Variant> _blackboard = new();
	
	public Dictionary<StringName, Variant> GetBlackboard()
	{
		return _blackboard.Duplicate();
	}

	public bool ValueEqual(StringName key, Variant compare)
	{
		Variant v = GetValue(key);

		if ((bool)v == false || GetValueType(key) != compare.GetType() || !v.Equals(compare))
		{
			return false;
		}

		return true;
	}

	public bool TypeEqual(StringName key, Variant compare)
	{
		Variant v = GetValue(key);

		return !((bool)v == false || GetValueType(key) != compare.GetType());
	}

	public bool HasValue(StringName key)
	{
		bool hasValue = _blackboard.ContainsKey(key);

		if (EnableDebug)
		{
			System.Diagnostics.Debug.Assert(hasValue, "Attempted to access value with key: " + key + " but key does not exist");
		}

		return hasValue;
	}

	public Variant GetValue(StringName key)
	{
		if (!HasValue(key))
		{
			return false;
		}

		return _blackboard[key];
	}

	public System.Type GetValueType(StringName key)
	{
		if (!HasValue(key))
		{
			return null;
		}

		return GetValue(key).GetType();
	}

	public bool SetValue(StringName key, Variant value)
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

	public bool EraseValue(StringName key)
	{
		if (!HasValue(key))
		{
			return false;
		}

		_blackboard[key] = default;
		EmitSignal(SignalName.ValueChanged, key, _blackboard[key]);
		return true;
	}

	public override void _EnterTree()
	{
		if (Blackboard != null && Blackboard is BlackboardAsset bb && bb.Entries.Count > 0)
		{
			_blackboard = bb.Entries.Duplicate();
		}
	}

	public override void _Ready()
	{
		base._Ready();
	}

	public override string[] _GetConfigurationWarnings()
	{
		Array<string> warnings = new();

		foreach (var key in _blackboard.Keys)
		{
			if (key is not null)
			{
				warnings.Add("Blackboard keys must be of type StringName.");
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