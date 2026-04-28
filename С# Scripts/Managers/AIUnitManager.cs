using Godot;
using InsideTheWar.Helpers;
using InsideTheWar.Singletons;
using System.Collections.Generic;

namespace InsideTheWar.Entities;

public partial class AIUnitManager : Node
{
    private Dictionary<int, List<AIUnit>> Units = new();

    public override void _Ready()
    {
        base._Ready();

        GlobalSignals.Instance.EntitySpawned += OnUnitSpawn;
        GlobalSignals.Instance.AIUnitReady += OnUnitReady;
    }

    private void OnUnitSpawn(ulong id, Vector2 currentPosition)
    {
        var obj = InstanceFromId(id);

        if (obj is AIUnit unit)
        {
            if (!Units.ContainsKey(unit.SquadId))
            {
                Units[unit.SquadId] = new List<AIUnit>();
            }

            Units[unit.SquadId].Add(unit);
        }
    }

    protected void OnUnitReady(AIUnit unit)
    {
        foreach (var u in Units[unit.SquadId])
        {
            if (u.CurrentState != UnitStates.Waiting) { return; }
        }

        var RandomWaitingTime = GameMath.GetRandomNumber(unit.MinWaitingTime, unit.MaxWaitingTime);

        var squadCenter = GameMath.CalculateSquadCenter(Units[unit.SquadId]);

        var squadTargetPosition = GameMath.GetRandomPointInCircle
        (squadCenter, unit.MovementRadiusMin, unit.MovementRadiusMax);

        var assigments = GameMath.AssignUnitsToPointsAlgorithm(Units[unit.SquadId], squadTargetPosition);

        foreach (var pair in assigments)
        {
            var u = pair.Key;
            var point = pair.Value;

            ((AIUnit)u).MoveTo(point, RandomWaitingTime);
        }
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        GlobalSignals.Instance.EntitySpawned -= OnUnitSpawn;
        GlobalSignals.Instance.AIUnitReady -= OnUnitReady;
    }

}
