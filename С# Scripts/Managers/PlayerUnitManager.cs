using Godot;
using InsideTheWar.Singletons;
using System.Linq;
using InsideTheWar.Entities;
using InsideTheWar.Helpers;

namespace InsideTheWar.Managers;

public partial class PlayerUnitManager : Node
{
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

        GlobalSignals.Instance.PlayerLeaderPositionChanged += OnLeaderPositionChanged;

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

        Vector2 squadCenter = Vector2.Zero;
        foreach (var unit in selectedUnits)
        {
            squadCenter += unit.GlobalPosition;
        }
        squadCenter /= selectedUnits.Count;

        if (squadCenter.DistanceTo(mousePosition) < _minSquadMoveDistance)
        {
            GD.Print("To Small Distance to Move");
            return;
        }

        var leader = selectedUnits[0];
        Vector2I targetCell = _gridManager.TargetCell(mousePosition);
        _gridManager.ClearCellOccupation(leader.SquadId);
        _gridManager.UpdateOccupation(leader);

        foreach (var unit in selectedUnits)
        {
            var offset = FormationMath.CalculateOffset(unit.Col, unit.Row,
            unit.FormationCols, unit.FormationRows, unit.FormationSpacing);

            unit.TargetPosition = mousePosition + offset;
        }
    }
    private void OnLeaderPositionChanged(Vector2 position, int vision, int squadId)
    {
        Vector2I targetCell = _gridManager.TargetCell(position);
        if (targetCell != _lastCell)
        {
            GlobalSignals.Instance.EmitSignal
            (GlobalSignals.SignalName.EntityMoved, squadId, targetCell, vision);
        }
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        GlobalSignals.Instance.PlayerLeaderPositionChanged -= OnLeaderPositionChanged;
    }

}
