using Godot;

namespace InsideTheWar.Data;

[GlobalClass]
public partial class AIUnitData : UnitData
{
    [Export] public float MinIdleTime { get; protected set; } = 2.0f;
    [Export] public float MaxIdleTime { get; protected set; } = 6.0f;
}
