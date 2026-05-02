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
    private Dictionary<int, PlayerSquad> _squadsById = new();

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
        base._Ready();

        GlobalSignals.Instance.EntitySpawned += OnUnitSpawn;
        GlobalSignals.Instance.RequestSquadUnits += OnSquadUnitsRequest;
        //GlobalSignals.Instance.PlayerLeaderPositionChanged += OnLeaderPositionChanged;

    }

    public void MoveSquadTo(Vector2 mousePosition)
    {
        var selectedUnits = _selectionManager.SelectedUnits;
        var allUnits = _unitsContainer.GetChildren().OfType<PlayerUnit>().ToArray();

        if (selectedUnits.Count == 0)
        {
            return;
        }

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

        //var leader = selectedUnits[0];
        //Vector2I targetCell = _gridManager.TargetCell(mousePosition);
        //_gridManager.ClearCellOccupation(leader.SquadId);
        //_gridManager.UpdateOccupation(leader);
        /*
        var squadDirection = (mousePosition - squadCenter).Normalized();
        var rotationAngle = squadDirection.Angle();
        foreach (var unit in selectedUnits)
        {
            var offset = GameMath.CalculateSquadOffset(unit.Col, unit.Row,
            unit.FormationCols, unit.FormationRows, unit.FormationSpacing);

            var rotatedOffset = offset.Rotated(rotationAngle);

            unit.TargetPosition = mousePosition + rotatedOffset;
        }
        */
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
    /*
    private void OnLeaderPositionChanged(Vector2 position, int vision, int squadId)
    {
        Vector2I targetCell = _gridManager.TargetCell(position);
        if (targetCell != _lastCell)
        {
            GlobalSignals.Instance.EmitSignal
            (GlobalSignals.SignalName.EntityMoved, squadId, targetCell, vision);
        }
    }
    */
    public override void _ExitTree()
    {
        base._ExitTree();
        GlobalSignals.Instance.EntitySpawned -= OnUnitSpawn;
        GlobalSignals.Instance.RequestSquadUnits -= OnSquadUnitsRequest;
        //GlobalSignals.Instance.PlayerLeaderPositionChanged -= OnLeaderPositionChanged;
    }
    

}
