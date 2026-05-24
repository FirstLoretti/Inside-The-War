using System.Collections.Generic;
using Godot;
using InsideTheWar.Helpers;
using InsideTheWar.Interfaces;
using InsideTheWar.Singletons;

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

    public override void _PhysicsProcess(double delta)
    {
        if (Debug?.IsEnabled == true)
        {
            QueueRedraw();
        }

        if (_currentTargetSquadId == -1) { return; }

        Unit fightingUnit = null;
        foreach (var unit in Units)
        {
            if (unit.CurrentState == UnitStates.Attacking)
            {
                fightingUnit = unit;
                break;
            }
        }
        
        if (fightingUnit == null) { return; }

        var newUnitPositions = GameMath.CalculateUnitPositions(Units, fightingUnit.GlobalPosition);
        foreach (var unitPosition in newUnitPositions)
        {
            var unit = unitPosition.Key;
            var targetPosition = unitPosition.Value;
            if (unit.CurrentState != UnitStates.Attacking)
            {
                unit.SetState(UnitStates.Moving, targetPosition);
            }
        }
        _currentTargetSquadId = -1;
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
            unit.SetState(UnitStates.Idle);
        }
    }

    protected void TargetUpdateAndCharge()
    {
        GlobalSignals.Instance.EmitRequestSquadUnits(_currentTargetSquadId, (enemyUnits) =>
        {
            //_chargeUpdateTimer = _chargeUpdateInterval;
            if (enemyUnits.Count == 0)
            {
                _currentTargetSquadId = -1;
                Idle();
                return;
            }

            //_chargeUpdateTimer = _chargeUpdateInterval;
            var enemySquadCenter = GameMath.CalculateSquadCenter(enemyUnits);
            _combatDirection = (enemySquadCenter - _centerAtBattleStart).Normalized();
            Charge(enemySquadCenter);
            WarnEnemyAboutAttack(enemyUnits);
        });
    }

    private void WarnEnemyAboutAttack(List<Unit> enemyUnits)
    {
        foreach (var enemyUnit in enemyUnits)
        {
            if (enemyUnit.CurrentState == UnitStates.Idle)
            {
                enemyUnit.SetState(UnitStates.BattleReady);
            }
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

            unit.SetState(UnitStates.Charging, targetPos);
        }
    }
}
