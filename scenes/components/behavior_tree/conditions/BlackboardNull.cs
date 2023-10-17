using Godot;
using Godot.Collections;
using System;
using System.Threading.Tasks;

namespace ProjectMina.BehaviorTree;

[Tool]
[GlobalClass]
public partial class BlackboardNull : Condition
{
	protected enum Operators
	{
		EQUAL,
		NOT_EQUAL
	}

	[Export] protected string BlackboardKey = "";
	[Export] protected Operators Comparison = 0;

	public override void _Ready()
	{
		base._Ready();
	}

	protected override Task<ActionStatus> _Tick(AICharacter character, BlackboardComponent blackboard)
	{
		if (EvaluateComparison(blackboard) && _childActions[0] is Action a)
		{
			Succeed();
			_childActions[0].Tick(character, blackboard);
		}
		else
		{
			Fail();
		}

		return Task.FromResult(Status);
	}

	private bool EvaluateComparison(BlackboardComponent blackboard)
	{
		Variant blackboardValue = blackboard.GetValue(BlackboardKey);
		bool isNull = true;

		switch (blackboardValue.VariantType)
		{
			case Variant.Type.Nil:
				isNull = true;
				break;
			case Variant.Type.NodePath:
				isNull = (NodePath)blackboardValue == new NodePath();
				break;
			case Variant.Type.Array:
				Array<Variant> a = (Array<Variant>)blackboardValue;
				isNull = a.Count == 0;
				break;
			case Variant.Type.Float:
				isNull = (float)blackboardValue == 0.0f;
				break;
			case Variant.Type.Int:
				isNull = (int)blackboardValue == 0;
				break;
			case Variant.Type.Vector2:
				isNull = (Vector2)blackboardValue == Vector2.Zero;
				break;
			case Variant.Type.Vector3:
				isNull = (Vector3)blackboardValue == Vector3.Zero;
				break;
			case Variant.Type.Bool:
				isNull = (bool)blackboardValue;
				break;
			default:
				break;
		}

		GD.Print("result: ", isNull);
		GD.Print("type: ", blackboardValue.VariantType);
		GD.Print("null: ", blackboardValue.VariantType == Variant.Type.Nil);

		switch (Comparison)
		{
			case Operators.NOT_EQUAL:
				return !isNull;
			default:
				return isNull;
		}

	}

	public override string[] _GetConfigurationWarnings()
	{
		Array<string> warnings = new();

		if (GetChildCount() != 1)
		{
			warnings.Add("Comparison node expected to have precisely one child.");
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
