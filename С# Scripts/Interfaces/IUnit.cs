using Godot;

namespace InsideTheWar.Interfaces;

public interface IUnit
{
    Vector2 GlobalPosition { get; }
    Vector2 MovementTargetPosition { get; }
    ulong Id { get; set; }
    int FormationCols { get; }
    int FormationRows { get; }
    int FormationSpacing { get; }
}
