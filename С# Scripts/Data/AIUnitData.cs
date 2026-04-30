using Godot;

namespace InsideTheWar.Data;

public partial class AIUnitData : BaseUnitData
{
    [Export] public float MinIdleTime = 2.0f;
    [Export] public float MaxIdleTime = 6.0f;
}
