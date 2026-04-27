using Godot;

namespace InsideTheWar.Data;

public partial class AIUnitData : BaseUnitData
{
    [Export] public float MinWaitingTime = 2.0f;
    [Export] public float MaxWaitingTime = 6.0f;
}
