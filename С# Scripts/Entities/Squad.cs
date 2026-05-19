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

    protected int _currentTargetSquadId = -1;
    protected Vector2 _combatDirection;
    protected Vector2 _centerAtBattleStart;

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

    public virtual void OnUnitDie(Unit unit)
    {
        UnitsCount -= 1;
        unit.Die -= OnUnitDie;
        Units.Remove(unit);
    }

    public virtual void RegisterUnit(Unit unit)
    {
        Units.Add(unit);
        unit.Die += OnUnitDie;
    }

    protected void Idle()
    {
        foreach (var unit in Units)
        {
            unit.Idle();
        }
    }

    public void Charge(Vector2 enemySquadCenter)
    {
        var leaderData = Units[0].Data;
        var attackPoint = enemySquadCenter - _combatDirection * leaderData.AttackDistance;
        var unitPositions = GameMath.CalculateUnitPositions(Units, attackPoint);

        foreach (var pair in unitPositions)
        {
            var unit = pair.Key as Unit;
            var targetPos = pair.Value;

            if (unit.CurrentState == UnitStates.Attacking) { continue; }

            unit.Charge(targetPos);
        }
    }
}
