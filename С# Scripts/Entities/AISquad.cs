using System.Collections.Generic;
using Godot;
using InsideTheWar.Helpers;

namespace InsideTheWar.Entities;

public partial class AISquad : Node
{
    public List<AIUnit> Units { get; set; } = new();
    private Node2D CurrentTarget { get; set; }

    public void OnEnemySpotted(Node2D enemy)
    {
        if (CurrentTarget == null)
        {
            CurrentTarget = enemy;

            foreach (var unit in Units)
            {
                unit.CurrentState = UnitStates.Waiting;
            }           
        }

        AttackTarget();
    }

    public void AttackTarget()
    {
        var attackPosition = CurrentTarget.GlobalPosition;

        var assigments = GameMath.AssignUnitsToPointsAlgorithm(Units, attackPosition);

        foreach (var pair in assigments)
        {
            var unit = pair.Key as AIUnit;
            var target = pair.Value;

            unit.MoveTo(target, 0.0f);
        }
    }

    public void OnUnitReady(AIUnit unit)
    {
        foreach (var u in Units)
        {
            if (u.CurrentState != UnitStates.Waiting) { return; }
        }

        var RandomWaitingTime = GameMath.GetRandomNumber(unit.MinWaitingTime, unit.MaxWaitingTime);

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
