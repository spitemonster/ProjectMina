using Godot;
using Godot.Collections;

namespace ProjectMina;

public partial class AudioManager : ManagerBase
{
    
    

    public override void _Ready()
    {
        CallDeferred("RegisterSelf");
        // RegisterSelf();
    }

    private void RegisterSelf()
    {
        if (Global.Data != null && Global.Data.AudioManager == null)
        {
            Global.Data.AudioManager = this;
        }

        GD.PushError("No global data for audio manager, dying");
        QueueFree();
    }
}

