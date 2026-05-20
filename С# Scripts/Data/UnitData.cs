using Godot;

namespace InsideTheWar.Data;

public partial class UnitData : EntityData
{
    [Export] public float MinSpeed { get; protected set; } = 150.0f;
    [Export] public float MaxSpeed { get; protected set; } = 125.0f;
    [Export] public float AttackDistance { get; protected set; } = 60.0f;
    [Export] public int MinDamage { get; protected set; } = 20;
    [Export] public int MaxDamage { get; protected set; } = 30;
}
