using Godot;
using InsideTheWar.Entities;
using InsideTheWar.Interfaces;
using InsideTheWar.Singletons;
using System.Collections.Generic;

namespace InsideTheWar.Managers;

public partial class AIUnitManager : Node
{
    private Dictionary<int, AISquad> SquadsById = new();

    private IDebug _debug;

    public override void _Ready()
    {
        GlobalSignals.Instance.EntitySpawned += OnUnitSpawn;
    }

    public void Init(IDebug debug)
    {
        _debug = debug;
    }

    // Создание отряда и назначение юнита в отряд
    private void OnUnitSpawn(ulong id, Vector2 currentPosition)
    {
        var obj = InstanceFromId(id);

        if (obj is AIUnit unit)
        {
            if (!SquadsById.ContainsKey(unit.SquadId))
            {
                AISquad newSquad = new();
                if (_debug != null)
                {
                    newSquad.Debug = _debug;
                }
                AddChild(newSquad);
                SquadsById[unit.SquadId] = newSquad;
                newSquad.ExpectedUnitsCount = unit.FormationCols * unit.FormationRows;

            }

            var currentSquad = SquadsById[unit.SquadId];
            currentSquad.Units.Add(unit);

            //! Отписаться!
            unit.ReadyToAct += currentSquad.OnUnitReady;
            unit.EnemySpotted += currentSquad.OnEnemySpotted;

            unit.ReportReady();
        }
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        GlobalSignals.Instance.EntitySpawned -= OnUnitSpawn;
    }

}
