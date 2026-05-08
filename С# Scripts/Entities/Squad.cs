using System;
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
    public event Action<IUnit> TargetedByCharge;

    private List<Unit> _enemyUnits = [];

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
                _enemyUnits.Clear();
                _enemyUnits.AddRange(enemyUnits);
                var enemyCenter = GameMath.CalculateSquadCenter(enemyUnits);
                ChargeTarget(enemyCenter);
            });
        }
    }
    
    public void ChargeTarget(Vector2 targetPosition)
    {
        var assigments = GameMath.AssignUnitsToPointsAlgorithm(Units, targetPosition);

        foreach (var pair in assigments)
        {
            var unit = pair.Key as AIUnit;
            var target = pair.Value;

            if (unit.CurrentState == UnitStates.Attacking) continue;

            unit.Charge(target);
        }
    }
    protected void OnTargetedByCharge(IUnit unit)
    {

    }
}
