using Godot;
using Godot.Collections;

namespace ProjectMina;

public partial class SavedGame : Resource
{
    [Export] public SaveDataGlobal GlobalData = new();
    [Export] public Array<SaveDataBase> GameData = new();
}