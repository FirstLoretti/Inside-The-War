using Godot;
using System.Linq;
using InsideTheWar.Entities;
using InsideTheWar.Helpers;
using System.Collections.Generic;
using InsideTheWar.Singletons;
using System;

namespace InsideTheWar.Managers;

public partial class PlayerUnitManager : UnitManager
{
    [ExportGroup("Nodes")]
    [Export] private Node2D _unitsContainer;

    [ExportGroup("Technical")]
    [Export] private float _minSquadMoveDistance = 50.0f;

    [ExportGroup("Managers")]
    [Export] private SelectionManager _selectionManager;

    private Vector2I _lastCell;

    public override void _Ready()
    {
        base._Ready();
        GlobalSignals.Instance.RequestSquadUnits += OnSquadUnitsRequest;
    }

    public void ChargeInputHandler(Vector2 mousePosition)
    {
        var selectedSquadId = _selectionManager.SelectedSquadId;
        if (selectedSquadId == -1) { return; }

        if (_squadsById.TryGetValue(selectedSquadId, out var squad))
        {
            if(squad is PlayerSquad playerSquad)
            {
                playerSquad.OnChargeInput(mousePosition, Constants.AIUnits);
            }
        }
    }

    public void MoveSquadTo(Vector2 mousePosition)
    {
        var selectedUnits = _selectionManager.SelectedUnits;
        var allUnits = _unitsContainer.GetChildren().OfType<PlayerUnit>().ToArray();

        if (selectedUnits.Count == Constants.Zero) { return; }

        foreach (var unit in allUnits)
        {
            if (unit.GlobalPosition.DistanceTo(mousePosition) < _selectionManager.UnitClickOverlapRadius)
            {
                GD.Print("Clicked on Unit");
                return;
            }
        }

        var squadCenter = GameMath.CalculateCenterMass(selectedUnits);

        if (squadCenter.DistanceTo(mousePosition) < _minSquadMoveDistance)
        {
            GD.Print("To Small Distance to Move");
            return;
        }

        var assigments = GameMath.CalculateUnitPositions(selectedUnits, mousePosition);

        foreach (var pair in assigments)
        {
            var unit = pair.Key as PlayerUnit;
            var point = pair.Value;
            unit.SetState(UnitStates.Moving, point);
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
        base._ExitTree();
        GlobalSignals.Instance.RequestSquadUnits -= OnSquadUnitsRequest;
    }
}
