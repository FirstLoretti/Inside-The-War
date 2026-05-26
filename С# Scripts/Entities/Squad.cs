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
    protected Vector2 _attackDirection;
    protected Vector2 _centerAtStartCharge;

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

        Unit unitInCombat = null;
        foreach (var unit in Units)
        {
            if (unit.CurrentState == UnitStates.Attacking)
            {
                unitInCombat = unit;
                break;
            }
        }

        if (unitInCombat == null) { return; }

        SetUnitPositionsAfterStartCombat(unitInCombat);
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
            _centerAtStartCharge = GameMath.CalculateCenterMass(Units);
            var enemySquadCenter = GameMath.CalculateCenterMass(enemyUnits);
            _attackDirection = (enemySquadCenter - _centerAtStartCharge).Normalized();
            Charge(enemySquadCenter);
            WarnEnemyAboutAttack(enemyUnits);
        });
    }

    private void SetUnitPositionsAfterStartCombat(Unit firstUnitInCombat)
    {
        var formationCenter = GameMath.CalculateFormationCenterFromFront(firstUnitInCombat, _attackDirection, _centerAtStartCharge);
        var newUnitPositions = GameMath.CalculateUnitPositions(Units, formationCenter);
        foreach (var unitPosition in newUnitPositions)
        {
            var unit = unitPosition.Key;
            var targetPosition = unitPosition.Value;
            if (unit.CurrentState != UnitStates.Attacking)
            {
                unit.SetState(UnitStates.Charging, targetPosition, _attackDirection);
            }
        }
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
        var attackPoint = enemySquadCenter - _attackDirection * leaderData.AttackDistance;
        var unitPositions = GameMath.CalculateUnitPositions(Units, attackPoint);

        foreach (var pair in unitPositions)
        {
            var unit = pair.Key as Unit;
            var targetPos = pair.Value;

            if (unit.CurrentState == UnitStates.Attacking) { continue; }

            unit.SetState(UnitStates.Charging, targetPos, _attackDirection);
        }
    }
}
