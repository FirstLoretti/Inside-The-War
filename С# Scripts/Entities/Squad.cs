using System.Collections.Generic;
using Godot;
using InsideTheWar.Helpers;
using InsideTheWar.Interfaces;

namespace InsideTheWar.Entities;

public partial class Squad : Node2D
{
    public List<Unit> Units { get; set; } = [];
    public int UnitsCount { get; set; }
    public IDebug Debug { get; set; }

    protected Unit _currentTarget;

    public override void _Ready()
    {
        AddToGroup(Constants.Debuggable);
    }

    public void Init(IDebug debug)
    {
        Debug = debug;
    }

    public override void _Process(double delta)
    {
        if (Debug?.IsEnabled == true)
        {
            QueueRedraw();
        }
    }

    public virtual void OnUnitDying(Unit unit)
    {
        unit.Dying -= OnUnitDying;
        UnitsCount -= 1;
        Units.Remove(unit);
    }

    public virtual void RegisterUnit(Unit unit)
    {
        Units.Add(unit);
        unit.Dying += OnUnitDying;
    }

    public void ChargeTarget(Vector2 enemySquadCenter)
    {
        var directionToEnemy = (enemySquadCenter - GameMath.CalculateSquadCenter(Units)).Normalized();
        var squadOffset = enemySquadCenter - directionToEnemy * Units[0].Stats.AttackDistance;
        var assigments = GameMath.AssignUnitsToPointsAlgorithm(Units, squadOffset);

        foreach (var pair in assigments)
        {
            var unit = pair.Key as AIUnit;
            var target = pair.Value;

            if (unit.CurrentState == UnitStates.Attacking) continue;

            unit.Charge(target);
        }
    }
}
