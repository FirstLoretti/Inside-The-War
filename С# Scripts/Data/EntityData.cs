using Godot;

namespace InsideTheWar.Data;

[GlobalClass]
public partial class EntityData : Resource
{
    [Export] public int Health { get; private set; } = 100;
    [Export] public int MaxAttackers { get; private set; } = 1;
    [Export] public int FogVisionDistance { get; private set; } = 1;
}
