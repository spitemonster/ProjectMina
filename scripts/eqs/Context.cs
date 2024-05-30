using Godot;
using Godot.Collections;
using System.Threading.Tasks;

namespace ProjectMina.EnvironmentQuerySystem;

// describes a list of points taht 
[Tool]
[GlobalClass]
public partial class Context : EQSNode
{

	// override this function to generate the array of points
	public virtual async Task<Array<Vector3>> GetPoints(AgentComponent querier)
	{
		return await Task.Run(() =>
		{
			return new Array<Vector3>();
		});
	}

	public override string[] _GetConfigurationWarnings()
	{
		Godot.Collections.Array<string> warnings = new();
		
		if (GetChildCount() == 0)
		{
			warnings.Add("An EQS Context must have at least one child Test node.");
		}
		
		foreach (var child in GetChildren())
		{
			if (child is not Test)
			{
				warnings.Add("An EQS Context may only have Test node children.");
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
