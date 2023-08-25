using Godot;
using Godot.Collections;
using System.Threading.Tasks;

namespace ProjectMina.BehaviorTree;

[Tool]
[GlobalClass]
public partial class BlackboardCompare : Condition
{
	protected enum Operators
	{
		EQUAL,
		NOT_EQUAL,
		GREATER,
		GREATER_EQUAL,
		LESS,
		LESS_EQUAL,
	}

	// [Export(PropertyHint.Expression)] 
	[Export] protected string BlackboardKey = "";
	[Export] protected Operators Comparison = 0;
	[Export(PropertyHint.Expression)] protected string Value = "";

	private Expression val;

	public override void _Ready()
	{
		base._Ready();
		val = _ParseExpression(Value);
	}

	protected override Task<ActionStatus> _Tick(AICharacter character, BlackboardComponent blackboard)
	{
		if (EvaluateComparison(blackboard))
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
		Variant compareValue = val.Execute(new(), val);
		Variant blackboardValue = blackboard.GetValue(BlackboardKey);
		bool result = false;

		if (val.HasExecuteFailed())
		{
			Fail();
		}

		switch (Comparison)
		{
			case Operators.EQUAL:
				result = blackboardValue.Equals(compareValue);
				break;
			case Operators.NOT_EQUAL:
				result = !blackboardValue.Equals(compareValue);
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
