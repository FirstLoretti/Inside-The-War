using Godot;
using InsideTheWar.Data;
using InsideTheWar.Entities;

namespace InsideTheWar.Interfaces;

public interface IUnit
{
    Vector2 GlobalPosition { get; }
    Vector2 MovementTargetPosition { get; }
    FormationData FormationData { get; }
    UnitStates CurrentState { get; }
    ulong Id { get; set; }
    void SetState(UnitStates unitState, Vector2? targetPosition = null, Vector2? lookDirectionInBattle = null);
}
