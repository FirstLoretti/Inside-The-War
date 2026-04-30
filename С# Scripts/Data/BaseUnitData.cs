using Godot;

namespace InsideTheWar.Data;

[GlobalClass]
public partial class BaseUnitData : Resource
{
    [Export] public float Health { get; private set; } = 100.0f;
    [Export] public float MinSpeed { get; private set; } = 150.0f;
    [Export] public float MaxSpeed { get; private set; } = 125.0f;
    // [Export] public int Vision = 1;
    [Export] public float AttackDistance = 60.0f;
    [Export] public float VisionDistance = 200.0f;
    //[Export] public float AvoidanceWeight = 0.2f;
}
