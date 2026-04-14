using Godot;
using InsideTheWar.Singletons;
using System.Linq;
using InsideTheWar.Entities;

namespace InsideTheWar.Managers;

public partial class PlayerUnitManager : Node
{
    [ExportGroup("Nodes")]
    [Export] private Node _unitsContainer;

    [ExportGroup("Technical")]
    [Export] private float _unitClickOverlapRadius = 25.0f;
    [Export] private float _minSquadMoveDistance = 50.0f;

    [ExportGroup("Managers")]
    [Export] private GridManager _gridManager;

    private Vector2I _lastCell;

    public override void _Ready()
    {
        base._Ready();

        Signals.Instance.PlayerLeaderPositionChanged += OnLeaderPositionChanged;

    }

    private void MoveSquadTo(Vector2 mousePosition)
    {
        var allUnits = _unitsContainer.GetChildren().OfType<PlayerUnit>().ToArray();
        var selectedUnits = allUnits.Where(unit => unit.IsSelected).ToArray();

        if (selectedUnits.Length == 0)
        {
            return;
        }

        foreach (var unit in allUnits)
        {
            if (unit.GlobalPosition.DistanceTo(mousePosition) < _unitClickOverlapRadius)
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
        squadCenter /= selectedUnits.Length;

        if (squadCenter.DistanceTo(mousePosition) < _minSquadMoveDistance)
        {
            GD.Print("To Small Distance to Move");
            return;
        }

        var leader = selectedUnits[0];
        Vector2I targetCell = _gridManager.TargetCell(mousePosition);
        _gridManager.ClearCellOccupation(leader.SquadId);
        _gridManager.UpdateOccupation(leader.GlobalPosition, targetCell, leader.SquadId);

        foreach (var unit in selectedUnits)
        {
            Vector2 offset = CalculateFormationOffset(unit);
            unit.MoveTo(mousePosition + offset);
        }
    }
    private void OnLeaderPositionChanged(Vector2 position, int vision, int squadId)
    {
        Vector2I targetCell = _gridManager.TargetCell(position);
        if( targetCell != _lastCell)
        {
            Signals.Instance.EmitSignal
            (Signals.SignalName.EntityMoved, squadId, targetCell, vision);
        }
    }
    private Vector2 CalculateFormationOffset(PlayerUnit playerUnit)
    {
        var c = playerUnit.MyCol;
        var r = playerUnit.MyRow;
        var s = playerUnit.FormationSpacing;
        var c_count = playerUnit.FormationCols;
        var r_count = playerUnit.FormationRows;

        float offset_x = (c - (c_count - 1.0f) / 2.0f) * s;
        float offset_y = (r - (r_count - 1.0f) / 2.0f) * s;

        return new Vector2(offset_x, offset_y);
    }

}
