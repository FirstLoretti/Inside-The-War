using System.Collections.Generic;
using Godot;
using InsideTheWar.Helpers;
using InsideTheWar.Singletons;

namespace InsideTheWar.Entities;

public partial class AISquad : Node2D
{
    public List<AIUnit> Units { get; set; } = new();

    public int ExpectedUnitsCount { get; set; }

    private Unit _currentTarget;

    public override void _Ready()
    {
        AddToGroup("Debuggable"); //! Create const
    }

    public override void _Process(double delta)
    {
        if (GlobalDebugManager.IsEnabled)
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

    public void OnUnitReady(AIUnit unit)
    {
        if (Units.Count < ExpectedUnitsCount) { return; } //! Сломается при убийстве

        foreach (var u in Units)
        {
            if (u.CurrentState != UnitStates.WaitingOrder) { return; }
        }

        var RandomWaitingTime = GameMath.GetRandomNumber(unit.MinIdleTime, unit.MaxIdleTime);

        var squadCenter = GameMath.CalculateSquadCenter(Units);
        var squadTargetPosition = GameMath.GetRandomPointInCircle(squadCenter, unit.MovementRadiusMin, unit.MovementRadiusMax);

        var assigments = GameMath.AssignUnitsToPointsAlgorithm(Units, squadTargetPosition);

        foreach (var pair in assigments)
        {
            var u = pair.Key as AIUnit;
            var point = pair.Value;

            u.MoveTo(point, RandomWaitingTime);
        }
    }

    public override void _Draw()
    {
        if (!GlobalDebugManager.IsEnabled || Units.Count == 0) return;

        var squadCenter = GameMath.CalculateSquadCenter(Units);
        var localCenter = ToLocal(squadCenter);

        //! Vision is a rectangle
        //! Doesn't work correctly if spacing is greater than 80
        DrawCircle(localCenter, Units[0].Stats.VisionDistance, Colors.Yellow with { A = 0.2f });
        DrawCircle(localCenter, Units[0].Stats.VisionDistance, Colors.Yellow with { A = 0.3f }, false, 2.0f);
    }

}
