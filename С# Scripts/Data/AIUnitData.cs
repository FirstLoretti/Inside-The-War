using Godot;

namespace InsideTheWar.Data;

public partial class AIUnitData : UnitData
{
    [Export] public float MinIdleTime { get; protected set; } = 2.0f;
    [Export] public float MaxIdleTime { get; protected set; } = 6.0f;
    [Export] public float VisionDistance { get; protected set; } = 200.0f;
}
