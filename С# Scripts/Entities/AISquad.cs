using System.Collections.Generic;
using Godot;
using InsideTheWar.Data;
using InsideTheWar.Helpers;
using InsideTheWar.Singletons;
namespace InsideTheWar.Entities;

public partial class AISquad : Squad
{
    private float _chargeUpdateInterval = 0.5f;
    private float _chargeUpdateTimer;

    public override void _Process(double delta)
    {
        base._Process(delta);
        var deltaFloat = (float)delta;

        //TickChargeTimer(deltaFloat);
    }

    public override void RegisterUnit(Unit unit)
    {
        base.RegisterUnit(unit);
        if (unit is AIUnit aiUnit)
        {
            aiUnit.ReadyToAct += OnUnitReady;
            aiUnit.EnemySpotted += OnEnemySpotted;

            aiUnit.ReportReady();
        }
    }

    public override void OnUnitDying(Unit unit)
    {
        base.OnUnitDying(unit);
        if (unit is AIUnit aiUnit)
        {
            aiUnit.ReadyToAct -= OnUnitReady;
            aiUnit.EnemySpotted -= OnEnemySpotted;
        }
    }

    public void OnEnemySpotted(Node2D enemy)
    {
        if (_currentTargetSquadId != -1 || enemy is not Unit enemyUnit) { return; }

        _currentTargetSquadId = enemyUnit.SquadId;
        _chargeUpdateTimer = _chargeUpdateInterval;
        _centerAtBattleStart = GameMath.CalculateSquadCenter(Units);

        TargetUpdateAndCharge();
    }

    private void TargetUpdateAndCharge()
    {
        GlobalSignals.Instance.EmitRequestSquadUnits(_currentTargetSquadId, (enemyUnits) =>
        {
            _chargeUpdateTimer = _chargeUpdateInterval;
            if (enemyUnits.Count == 0)
            {
                _currentTargetSquadId = -1;
                Idle();
                return;
            }
            
            _chargeUpdateTimer = _chargeUpdateInterval;
            var enemySquadCenter = GameMath.CalculateSquadCenter(enemyUnits);
            _combatDirection = (enemySquadCenter - _centerAtBattleStart).Normalized();
            Charge(enemySquadCenter);
            WarnEnemyAboutAttack(enemyUnits);
        });
    }

    private void TickChargeTimer(float delta)
    {
        if (_currentTargetSquadId == -1) { return; }

        _chargeUpdateTimer -= delta;
        if (_chargeUpdateTimer <= Constants.Zero)
        {
            TargetUpdateAndCharge();
            _chargeUpdateTimer = _chargeUpdateInterval;
        }
    }

    private void WarnEnemyAboutAttack(List<Unit> enemyUnits)
    {
        foreach (var enemyUnit in enemyUnits)
        {
            if (enemyUnit.CurrentState == UnitStates.Idle)
            {
                enemyUnit.BattleReady();
            }
        }
    }

    public void OnUnitReady(AIUnit unit)
    {
        if (Units.Count < UnitsCount) { return; }

        foreach (var u in Units)
        {
            if (u.CurrentState != UnitStates.WaitingOrder) { return; }
        }

        var RandomWaitingTime = GameMath.GetRandomNumber(unit.MinIdleTime, unit.MaxIdleTime);

        var squadCenter = GameMath.CalculateSquadCenter(Units);
        var squadTargetPosition = GameMath.GetRandomPointInCircle(squadCenter, unit.MovementRadiusMin, unit.MovementRadiusMax);

        var assigments = GameMath.CalculateUnitPositions(Units, squadTargetPosition);

        foreach (var pair in assigments)
        {
            var u = pair.Key as AIUnit;
            var point = pair.Value;

            u.MoveTo(point, RandomWaitingTime);
        }
    }

    public override void _Draw()
    {
        if (Debug?.IsEnabled == true && Units.Count > 0)
        {
            var squadCenter = GameMath.CalculateSquadCenter(Units);
            var localCenter = ToLocal(squadCenter);
            var stats = (AIUnitData)Units[0].Stats;
            //! Vision is a rectangle
            //! Doesn't work correctly if spacing is greater than 80
            DrawCircle(localCenter, stats.VisionDistance, Colors.Yellow with { A = 0.2f });
            DrawCircle(localCenter, stats.VisionDistance, Colors.Yellow with { A = 0.3f }, false, 2.0f);
        }
    }
}
