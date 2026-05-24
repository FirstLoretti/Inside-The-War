using Godot;

namespace InsideTheWar.Data;

[GlobalClass]
public partial class PlayerUnitData : UnitData
{
    [Export] public int FogVisionDistance { get; protected set; } = 1;
}
