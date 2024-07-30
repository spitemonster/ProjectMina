using Godot;
using Godot.Collections;
using Microsoft.VisualBasic;

namespace ProjectMina;

[Tool]
[GlobalClass,Icon("res://_dev/icons/icon--board.svg")]
public partial class BlackboardComponent : ComponentBase
{
	[Signal] public delegate void ValueChangedEventHandler(StringName key, Variant newValue);
	[Export] public BlackboardAsset Blackboard { get; protected set; }

	public Array<StringName> Keys = new();

	public Dictionary<StringName, Variant> GetEntries()
	{
		return Blackboard.Entries.Duplicate();
	}

	public bool ValueEqual(StringName key, Variant compare)
	{
		Variant v = GetValue(key);

		if (v.Equals(compare) || (string)v == (string)compare || (Vector3)v == (Vector3)compare || (bool)v == (bool)compare || (float)v == (float)compare)
		{
			return true;
		}

		return false;
	}

	public bool TypeEqual(StringName key, Variant compare)
	{
		Variant v = GetValue(key);

		return !((bool)v == false || GetValueType(key) != compare.VariantType);
	}

	public bool HasValue(StringName key)
	{
		bool hasValue = Blackboard.Entries.ContainsKey(key);

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

		return Blackboard.Entries[key];
	}

	public bool GetValueAsBool(StringName key)
	{
		return GetValue(key).AsBool();
	}

	public bool SetValueAsBool(StringName key, bool value)
	{
		return SetValue(key, value);
	}

	public Vector3 GetValueAsVector3(StringName key)
	{
		return GetValue(key).AsVector3();
	}

	public bool SetValueAsVector3(StringName key, Vector3 value)
	{
		return SetValue(key, value);
	}

	public bool SetValueAsInt(StringName key, int value)
	{
		return SetValue(key, value);
	}

	public int GetValueAsInt(StringName key)
	{
		return GetValue(key).AsInt32();
	}

	public float GetValueAsFloat(StringName key)
	{
		return (float)GetValue(key).AsDouble();
	}

	public GodotObject GetValueAsObject(StringName key)
	{
		return GetValue(key).AsGodotObject();
	}

	public bool SetValueAsObject(StringName key, GodotObject value)
	{
		return SetValue(key, value);
	}

	public string GetValueAsString(StringName key)
	{
		return GetValue(key).AsString();
	}

	public bool SetValueAsString(StringName key, string value)
	{
		return SetValue(key, value);
	}

	public Variant.Type GetValueType(StringName key)
	{
		if (!HasValue(key))
		{
			return Variant.Type.Nil;
		}

		return GetValue(key).VariantType;
	}

	public bool SetValue(StringName key, Variant value)
	{
		// ensure our data contains the correct key, the types are the same and the value currently there is not the same as what is being submitted
		if (!HasValue(key) || Blackboard.Entries[key].GetType() != value.GetType() || Blackboard.Entries[key].Equals(value))
		{
			return false;
		}
		
		Blackboard.Entries[key] = value;
		EmitSignal(SignalName.ValueChanged, key, value);
		return true;
	}

	public bool EraseValue(StringName key)
	{
		if (!HasValue(key))
		{
			return false;
		}

		Blackboard.Entries[key] = default;
		EmitSignal(SignalName.ValueChanged, key, Blackboard.Entries[key]);
		return true;
	}

	public override void _EnterTree()
	{
		if (Blackboard != null && Blackboard is BlackboardAsset bb && bb.Entries.Count > 0)
		{
			Blackboard.Entries = bb.Entries.Duplicate();
		}
	}

	public override void _Ready()
	{
		base._Ready();
	}
}