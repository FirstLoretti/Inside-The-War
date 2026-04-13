using Godot;
using InsideTheWar.Singletons;
using System.Linq;

namespace InsideTheWar.Entities;

public partial class PlayerUnitManager : Node
{
    [Export] private Node _unitsContainer;
    [Export] private float _unitClickOverlapRadius = 50.0f;
    [Export] private float _minSquadMoveDistance = 25.0f;

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
            if (unit.GlobalPosition.DistanceTo(mousePosition) < _minSquadMoveDistance)
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

        if (squadCenter.DistanceTo(mousePosition) < _unitClickOverlapRadius)
        {
            GD.Print("To Small Distance to Move");
            return;
        }
    }
    private void OnLeaderPositionChanged(Vector2 position, int Vision, int squad_id)
    {

    }

}
