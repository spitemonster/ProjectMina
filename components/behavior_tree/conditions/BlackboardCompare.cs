using Godot;
using Godot.Collections;
using System;
using System.Threading.Tasks;

namespace ProjectMina.BehaviorTree;

[Tool]
[GlobalClass]
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
	
	[Export] protected Operators Comparison = Operators.Equal;
	[Export(PropertyHint.Expression)] protected string Value = "";

	private Expression val;
	private bool _evaluation;

	public override void _Ready()
	{
		base._Ready();
		val = _ParseExpression(Value);
	}

	protected override EActionStatus _Tick(AIControllerComponent controller, BlackboardComponent blackboard)
	{
		if (ChildActions.Count < 1)
		{
			GD.PushError("no child actions");
			SetStatus(EActionStatus.Failed);
			return Status;
		}

		bool evalResult = EvaluateComparison(controller, blackboard);
		
		// await ToSignal(this, SignalName.ExpressionEvaluated);
		
		if (!_evaluation)
		{
			SetStatus(EActionStatus.Failed);
			return Status;
		}
		
		GD.Print("should run child");
		ChildActions[0].Tick(controller, blackboard);
		
		SetStatus(EActionStatus.Succeeded);
		return Status;
	}

	private bool EvaluateComparison(AIControllerComponent controller, BlackboardComponent blackboard)
	{

		if (val == null)
		{
			GD.Print("Shit!");
			return false;
		}
		
		GD.Print("original compare value: ", Value);
		GD.Print("val: ", val);
		Variant compareValue = val.Execute(new(), val);
		GD.Print("compare value: ", compareValue);
		Variant blackboardValue = blackboard.GetValue(BlackboardKey);
		bool result = false;

		GD.Print("blackboard value: ", blackboardValue);
		if (val.HasExecuteFailed())
		{
			GD.Print("fail");
			GD.Print(val.GetErrorText());
			SetStatus(EActionStatus.Failed);
		}

		switch (Comparison)
		{
			case Operators.Equal:
				result = blackboardValue.Equals(compareValue) || (string)blackboardValue == (string)compareValue;
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

		_evaluation = result;
		GD.Print("comparison result: ", result);
		return result;
	}

	private Expression _ParseExpression(string source)
	{
		Expression exp = new();
		Error res = exp.Parse(source);

		if (res != Error.Ok)
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
