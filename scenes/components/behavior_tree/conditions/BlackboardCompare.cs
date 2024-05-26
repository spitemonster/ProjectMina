using Godot;
using Godot.Collections;
using System;
using System.Threading.Tasks;

namespace ProjectMina.BehaviorTree;

public partial class BlackboardCompare : Condition
{
	protected enum Operators: int
	{
		Equal,
		NotEqual,
		Greater,
		GreaterEqual,
		Less,
		LessEqual,
	}

	// [Export(PropertyHint.Expression)] 
	[Export] protected string BlackboardKey = "";
	[Export] protected Operators Comparison = Operators.Equal;
	[Export(PropertyHint.Expression)] protected string Value = "";

	private Expression val;

	public override void _Ready()
	{
		base._Ready();
		val = _ParseExpression(Value);
	}

	protected override async Task<ActionStatus> _Tick(AgentComponent agent, BlackboardComponent blackboard)
	{
		return await Task.Run(() =>
		{
			if (EvaluateComparison(blackboard))
			{
				Succeed();
				_childActions[0].Tick(agent, blackboard);
			}
			else
			{
				Fail();
			}

			return Task.FromResult(Status);
		});
	}

	private bool EvaluateComparison(BlackboardComponent blackboard)
	{
		Variant compareValue = val.Execute(new(), val);
		Variant blackboardValue = blackboard.GetValue(BlackboardKey);
		bool result = false;

		if (val.HasExecuteFailed())
		{
			Fail();
		}

		switch (Comparison)
		{
			case Operators.Equal:
				result = blackboardValue.Equals(compareValue);
				break;
			case Operators.NotEqual:
				result = !blackboardValue.Equals(compareValue);
				break;
			case Operators.LessEqual:
				result = (float)compareValue <= (float)blackboardValue;
				break;
			case Operators.Less:
				result = (float)compareValue < (float)blackboardValue;
				break;
			case Operators.GreaterEqual:
				result = (float)compareValue >= (float)blackboardValue;
				break;
			case Operators.Greater:
				result = (float)compareValue > (float)blackboardValue;
				break;
			default:
				result = false;
				break;
		}

		return result;
	}

	private Expression _ParseExpression(string source)
	{
		Expression exp = new();
		Error res = exp.Parse(source);

		if (!(res == Error.Ok))
		{
			return null;
		}

		return exp;
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
