using System.Collections.Generic;
using System.Linq;
using Godot;
using InsideTheWar.Entities;

namespace InsideTheWar.Managers;

public partial class SelectionManager : Node
{
    [Export] private Node2D _playerUnitsContainer;
    [Export] private int _unitClickOverlapRadius = 25;

    public int UnitClickOverlapRadius => _unitClickOverlapRadius;

    private List<PlayerUnit> _selectedUnits = new();
    public IReadOnlyList<PlayerUnit> SelectedUnits => _selectedUnits;

    public void SelectSquad(Vector2 mousePos)
    {
        var allUnits = _playerUnitsContainer.GetChildren().OfType<PlayerUnit>().ToArray();
        foreach (var unit in allUnits)
        {
            unit.IsSelected = false;
        }
        _selectedUnits.Clear();

        var clickedUnit = allUnits.FirstOrDefault(u => u.GlobalPosition.DistanceTo(mousePos) < _unitClickOverlapRadius);

        if (clickedUnit != null)
        {
            var targetId = clickedUnit.SquadId;
            var squad = allUnits.Where(u => u.SquadId == targetId);

            foreach (var unit in squad)
            {
                unit.IsSelected = true;
                _selectedUnits.Add(unit);
            }
        }

        GD.Print($"Count of selected units: {_selectedUnits.Count}");
    }
}
