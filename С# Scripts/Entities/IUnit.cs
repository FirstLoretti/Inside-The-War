using Godot;

namespace InsideTheWar.Entities;

public interface IUnit
{
    Vector2 GlobalPosition { get; }
    Vector2 TargetPosition { get; set; }
    ulong Id { get; set; }
    int FormationCols { get; }
    int FormationRows { get; }
    int FormationSpacing { get; }
}
