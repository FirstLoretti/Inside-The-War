using Godot;

namespace InsideTheWar.Data;

public partial class AIUnitData : UnitData
{
    [Export] public float MinIdleTime { get; private set; } = 2.0f;
    [Export] public float MaxIdleTime { get; private set; } = 6.0f;
    [Export] public float VisionDistance { get; private set; } = 200.0f;
}
