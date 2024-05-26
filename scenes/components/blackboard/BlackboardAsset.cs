using Godot;
using Godot.Collections;

namespace ProjectMina;

[Tool]
[GlobalClass]
public partial class BlackboardAsset : Resource
{
	
	[Export] public Dictionary<StringName, Variant> Entries = new();
}
