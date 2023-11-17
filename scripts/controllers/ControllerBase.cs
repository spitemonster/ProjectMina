using Godot;

[Tool]
[GlobalClass]
public partial class ControllerBase : Node
{
	[Signal] public delegate void CharacterPossessedEventHandler();

	public override void _Ready()
	{

	}

	public void Possess()
	{

		// EmitSignal()
	}

	public override void _Process(double delta)
	{
	}

	// public override string[] _GetConfigurationWarnings()
	// {
	// 	// Array<string> warnings = new();

	// 	// if (Owner is not CharacterBase)
	// 	// {
	// 	// 	warnings.Add("Controller must be a child of the CharacterBase class");
	// 	// }

	// 	// string[] errs = new string[warnings.Count];

	// 	// for (int i = 0; i < warnings.Count; i++)
	// 	// {
	// 	// 	errs.SetValue(warnings[i], i);
	// 	// }

	// 	return string[];
	// }
}
