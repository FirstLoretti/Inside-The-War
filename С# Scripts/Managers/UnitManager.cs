using System.Collections.Generic;
using Godot;
using InsideTheWar.Entities;
using InsideTheWar.Helpers;
using InsideTheWar.Interfaces;
using InsideTheWar.Singletons;

namespace InsideTheWar.Managers;

public partial class UnitManager : Node
{
    protected Dictionary<int, Squad> _squadsById = [];

    private IDebug _debug;

    public override void _Ready()
    {
        GlobalSignals.Instance.EntitySpawned += OnUnitSpawn;
    }

    public void Init(IDebug debug) // Для передачи дебага отряду
    {
        _debug = debug;
    }

    private void OnUnitSpawn(ulong id, Vector2 currentPosition, StringName unitsGroup, StringName enemyGroup) // Создание отряда и назначение юнита в отряд
    {
        var obj = InstanceFromId(id);

        if (obj is Unit unit)
        {
            if (!_squadsById.ContainsKey(unit.SquadId))
            {
                var newSquad = CreateSquad(unitsGroup);

                newSquad.Debug = _debug;
                newSquad.Name= unit.SquadId.ToString();
                AddChild(newSquad);

                _squadsById[unit.SquadId] = newSquad;
                var formationData = unit.FormationData;
                newSquad.UnitsCount = formationData.Cols * formationData.Rows;
            }

            var currentSquad = _squadsById[unit.SquadId];
            currentSquad.RegisterUnit(unit);
        }
    }

    private Squad CreateSquad(StringName unitsGroup)
    {
        if (unitsGroup == Constants.PlayerUnits)
        {
            PlayerSquad playerSquad = new();
            return playerSquad;
        }
        if (unitsGroup == Constants.AIUnits)
        {
            AISquad aiSquad = new();
            return aiSquad;
        }

        return null;
    }

    public override void _ExitTree()
    {
        GlobalSignals.Instance.EntitySpawned -= OnUnitSpawn;
    }
}