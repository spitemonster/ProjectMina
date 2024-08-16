using Godot;
using Godot.Collections;

namespace ProjectMina;

[GlobalClass]
public partial class CharacterAnimationTree : AnimationTree
{
    // intended function
    // string name key is the name of a global parameter
    // the array key is a list of parameters in the anim tree that you would like to subscribe
    // when a global parameter is set with the "set global parameter" methods, all of the subscribed params will be set with it 
    [Export] public Dictionary<StringName, Array<string>> ParameterSubscriptions;

    public override void _Ready()
    {
        base._Ready();

        // ParameterSubscriptions = new();
    }

    public void SetGlobalFloatParameter(StringName parameter, float value)
    {
        if (ParameterSubscriptions == null || !ParameterSubscriptions.ContainsKey(parameter))
        {
            return;
        }
        
        var treeParams = ParameterSubscriptions[parameter];

        foreach (var treeParam in treeParams)
        {
            Set(treeParam, value);
        }
    }
}
