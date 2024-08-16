using Godot;
using System;

public partial class ItemData : Resource
{
    [Export] public string ItemName { get; private set; }
    [Export] public PackedScene ItemScene { get; private set; }
}
