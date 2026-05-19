using Godot;

namespace InsideTheWar.Data;

public partial class UnitData : EntityData
{
    [Export] public float MinSpeed { get; private set; } = 150.0f;
    [Export] public float MaxSpeed { get; private set; } = 125.0f;
    [Export] public float AttackDistance { get; private set; } = 60.0f;
    [Export] public int MinDamage { get; private set; } = 20;
    [Export] public int MaxDamage { get; private set; } = 30;
}
