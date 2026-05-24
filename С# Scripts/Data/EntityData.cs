using Godot;

namespace InsideTheWar.Data;

[GlobalClass]
public partial class EntityData : Resource
{
    [Export] public int Health { get; protected set; } = 100;
    [Export] public int MaxAttackers { get; protected set; } = 1;
}
