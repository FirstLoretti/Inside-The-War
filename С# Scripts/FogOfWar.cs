using System.Collections.Generic;
using System.Linq;
using Godot;
using InsideTheWar.Helpers;
using InsideTheWar.Singletons;

namespace InsideTheWar;

public partial class FogOfWar : Node
{
    [Export] private TileMapLayer _mainMap;
    [Export] private TileMapLayer _fogUnexplored;
    [Export] private TileMapLayer _fogExplored;

    private HashSet<Vector2I> _exploredCells = new();
    private Dictionary<ulong, (Vector2I currentCell, int vision)> _unitVisionById = new();

    public override void _Ready()
    {
        base._Ready();
        GlobalSignals.Instance.EntityMoved += OnEntityMoved;
    }

    private void OnEntityMoved(ulong id, Vector2 oldPosition, Vector2 currentPosition, int vision)
    {
        var currentCell = _mainMap.LocalToMap(currentPosition);
        _unitVisionById[id] = (currentCell, vision);

        RevealNewTerritory(currentCell, vision);

        UpdateDynamicVision();
        UpdateAIVisibility();
    }

    private void RevealNewTerritory(Vector2I center, int radius)
    {
        foreach (var c in GameMath.GetCellsInRadius(center, radius))
        {
            if (!_exploredCells.Contains(c))
            {
                _fogUnexplored.SetCell(c, -1);
                _exploredCells.Add(c);
            }
        }
    }

    private void UpdateDynamicVision() //! Очень тяжело!
    {
        foreach (var cell in _exploredCells)
        {
            _fogExplored.SetCell(cell, 0, Vector2I.Zero);
        }

        foreach (var visionData in _unitVisionById.Values)
        {
            foreach (var cell in GameMath.GetCellsInRadius(visionData.currentCell, visionData.vision))
            {
                _fogExplored.SetCell(cell, -1);
            }
        }
    }

    private void UpdateAIVisibility() //! Очень тяжело!
    {
        var aiUnits = GetTree().GetNodesInGroup("AIUnits");

        foreach (Node2D unit in aiUnits)
        {
            var cell = _mainMap.LocalToMap(unit.GlobalPosition);
            var isCellVisible = _fogExplored.GetCellSourceId(cell) == -1;

            var sprite = unit.GetNodeOrNull<Sprite2D>("Sprite2D");
            if(sprite != null)
            {
                sprite.Visible = isCellVisible;
            }
        }
    }
    /*
    private void RevealAndRemember(Vector2I currentCell, int vision)
    {
        foreach (var cell in GameMath.GetCellsInRadius(currentCell, vision))
        {
            _fogUnexplored.SetCell(cell, 0, Vector2I.Zero);
            _exploredCells.Add(cell);
        }
    }

    private void RefreshVision()
    {
        foreach (var cell in _exploredCells)
        {
            _fogExplored.SetCell(cell, 0, new Vector2I(0, 0));

        }

    }
    */
    public override void _ExitTree()
    {
        base._ExitTree();
        GlobalSignals.Instance.EntityMoved -= OnEntityMoved;
    }

}
