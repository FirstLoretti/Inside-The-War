using Godot;
using InsideTheWar.Data;

namespace InsideTheWar.Interfaces;

public interface IUnit
{
    Vector2 GlobalPosition { get; }
    Vector2 MovementTargetPosition { get; }
    ulong Id { get; set; }
    FormationData FormationData { get; }
}
