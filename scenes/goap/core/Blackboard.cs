using Godot;
using Godot.Collections;

namespace ProjectMina.Goap;
[GlobalClass]
public partial class Blackboard : Resource
{
	[Export] public Array<WorldProperty> BlackboardProperties;
}
