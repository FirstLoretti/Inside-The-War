using System.Collections.Generic;
using Godot;
using InsideTheWar.Helpers;
using InsideTheWar.Interfaces;
using InsideTheWar.Singletons;

namespace InsideTheWar.Entities;

public partial class Squad : Node2D
{
    public List<Unit> Units { get; set; } = [];
    public int ExpectedUnitsCount { get; set; }
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
        if (Debug.IsEnabled)
        {
            QueueRedraw();
        }
    }

    public void OnEnemySpotted(Node2D enemy)
    {
        if (_currentTarget != null || enemy is not Unit enemyUnit) { return; }
        {
            _currentTarget = enemyUnit;

            GlobalSignals.Instance.EmitRequestSquadUnits(enemyUnit.SquadId, (enemyUnits) =>
            {
                var enemySquadCenter = GameMath.CalculateSquadCenter(enemyUnits);
                ChargeTarget(enemySquadCenter);

                foreach (var enemyUnit in enemyUnits)
                {
                    if (enemyUnit.CurrentState == UnitStates.Idle)
                    {
                        enemyUnit.BattleReady();
                    }
                }
            });
        }
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
