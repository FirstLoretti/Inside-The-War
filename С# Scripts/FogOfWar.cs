using System.Collections.Generic;
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
    //private Dictionary<int, (Vector2I currentCell, int vision)> _squadVision = new();

    public override void _Ready()
    {
        base._Ready();
        GlobalSignals.Instance.EntityMoved += OnEntityMoved;
    }

    private void OnEntityMoved(ulong id, Vector2 oldPos, Vector2 currentPos, int vision)
    {
        
        var oldCell = _mainMap.LocalToMap(oldPos);
        var currentCell = _mainMap.LocalToMap(currentPos);

        foreach (var cell in GameMath.GetCellsInRadius(oldCell, vision))
        {
            _fogExplored.SetCell(cell, 0, Vector2I.Zero); 
        }

        foreach (var cell in GameMath.GetCellsInRadius(currentCell, vision))
        {
            _fogUnexplored.SetCell(cell, -1); 

            _exploredCells.Add(cell);

            _fogExplored.SetCell(cell, -1);
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
