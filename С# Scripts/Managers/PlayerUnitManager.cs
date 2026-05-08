using Godot;
using System.Linq;
using InsideTheWar.Entities;
using InsideTheWar.Helpers;
using System.Collections.Generic;
using InsideTheWar.Singletons;
using System;

namespace InsideTheWar.Managers;

public partial class PlayerUnitManager : Node
{
    private Dictionary<int, PlayerSquad> _squadsById = [];

    [ExportGroup("Nodes")]
    [Export] private Node2D _unitsContainer;

    [ExportGroup("Technical")]
    [Export] private float _minSquadMoveDistance = 50.0f;

    [ExportGroup("Managers")]
    [Export] private GridManager _gridManager;
    [Export] private SelectionManager _selectionManager;

    private Vector2I _lastCell;

    public override void _Ready()
    {
        GlobalSignals.Instance.EntitySpawned += OnUnitSpawn;
        GlobalSignals.Instance.RequestSquadUnits += OnSquadUnitsRequest;
    }

    public void MoveSquadTo(Vector2 mousePosition)
    {
        var selectedUnits = _selectionManager.SelectedUnits;
        var allUnits = _unitsContainer.GetChildren().OfType<PlayerUnit>().ToArray();

        if (selectedUnits.Count == Constants.Zero) {return;}

        foreach (var unit in allUnits)
        {
            if (unit.GlobalPosition.DistanceTo(mousePosition) < _selectionManager.UnitClickOverlapRadius)
            {
                GD.Print("Clicked on Unit");
                return;
            }
        }

        var squadCenter = GameMath.CalculateSquadCenter(selectedUnits);

        if (squadCenter.DistanceTo(mousePosition) < _minSquadMoveDistance)
        {
            GD.Print("To Small Distance to Move");
            return;
        }

        var assigments = GameMath.AssignUnitsToPointsAlgorithm(selectedUnits, mousePosition);

        foreach (var pair in assigments)
        {
            var unit = pair.Key as PlayerUnit;
            var point = pair.Value;
            unit.MoveTo(point);
        }
    }

    private void OnUnitSpawn(ulong id, Vector2 currentPosition)
    {
        var obj = InstanceFromId(id);

        if (obj is PlayerUnit unit)
        {
            if (!_squadsById.ContainsKey(unit.SquadId))
            {
                PlayerSquad newSquad = new();
                _squadsById[unit.SquadId] = newSquad;
            }

            var currentSquad = _squadsById[unit.SquadId];
            currentSquad.Units.Add(unit);
        }
    }

    private void OnSquadUnitsRequest(int squadId, Action<List<Unit>> callback)
    {
        if (_squadsById.ContainsKey(squadId))
        {
            callback?.Invoke(_squadsById[squadId].Units);
        }
    }

    public override void _ExitTree()
    {
        GlobalSignals.Instance.EntitySpawned -= OnUnitSpawn;
        GlobalSignals.Instance.RequestSquadUnits -= OnSquadUnitsRequest;
    }
    
}
