using Godot;
using InsideTheWar.Data;
using InsideTheWar.Helpers;
namespace InsideTheWar.Entities;

public partial class AISquad : Squad
{
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
        if (!Debug.IsEnabled || Units.Count == 0) return;

        var squadCenter = GameMath.CalculateSquadCenter(Units);
        var localCenter = ToLocal(squadCenter);
        var stats = (AIUnitData)Units[0].Stats;
        //! Vision is a rectangle
        //! Doesn't work correctly if spacing is greater than 80
        DrawCircle(localCenter, stats.VisionDistance, Colors.Yellow with { A = 0.2f });
        DrawCircle(localCenter, stats.VisionDistance, Colors.Yellow with { A = 0.3f }, false, 2.0f);
    }

}
