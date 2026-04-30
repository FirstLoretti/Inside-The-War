using System.Collections.Generic;
using Godot;
using InsideTheWar.Helpers;

namespace InsideTheWar.Entities;

public partial class AISquad : Node
{
    public List<AIUnit> Units { get; set; } = new();

    public int ExpectedUnitsCount { get; set; }

    private Node2D _currentTarget;

    public void OnEnemySpotted(Node2D enemy)
    {
        if (_currentTarget == null)
        {
            _currentTarget = enemy;

            foreach (var unit in Units)
            {
                unit.CurrentState = UnitStates.WaitingOrder;
            }
        }

        ChargeTarget();
    }

    public void ChargeTarget()
    {
        var attackPosition = _currentTarget.GlobalPosition;

        var assigments = GameMath.AssignUnitsToPointsAlgorithm(Units, attackPosition);

        foreach (var pair in assigments)
        {
            var unit = pair.Key as AIUnit;
            var target = pair.Value;

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

}
