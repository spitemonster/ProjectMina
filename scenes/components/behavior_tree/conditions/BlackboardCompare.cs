using Godot;
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

	protected override async Task<ActionStatus> _Tick(AICharacter character, BlackboardComponent blackboard)
	{
		Variant compareValue = val.Execute(new(), val);
		Variant blackboardValue = blackboard.GetValue(BlackboardKey);

		if (val.HasExecuteFailed())
		{
			Fail();
			return Status;
		}

		bool result = false;

		switch (Comparison)
		{
			case Operators.EQUAL:
				result = blackboardValue.Equals(compareValue);
				break;
			case Operators.NOT_EQUAL:
				result = !blackboardValue.Equals(compareValue);
				break;
		}

		if (result)
		{
			Succeed();
		}
		else
		{
			Fail();
		}

		return Status;
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
}
