using System.Collections.Generic;
using Godot;
using InsideTheWar.Entities;
using InsideTheWar.Singletons;

namespace InsideTheWar.Managers;

public partial class GridManager : Node2D
{
    [Export] private TileMapLayer _mainMap;

    private Dictionary<Vector2I, int> _occupiedCells = new Dictionary<Vector2I, int>();

    public Vector2 TargetPixels(Vector2I targetCell) => _mainMap.MapToLocal(targetCell);

    public Vector2I TargetCell(Vector2 targetPixels) => _mainMap.LocalToMap(targetPixels);

    public bool IsCellOccupied(Vector2I cellCoords) => _occupiedCells.ContainsKey(cellCoords);

    public void SetOccupied(Vector2 pos, int entityId) => _occupiedCells[TargetCell(pos)] = entityId;


    public override void _Ready()
    {
        base._Ready();

        GlobalSignals.Instance.UnitSpawned += UpdateOccupation;
    }

    public void UpdateOccupation(Unit unit)
    {
        Vector2I oldCell = TargetCell(unit.LastPosition);
        Vector2I newCell = TargetCell(unit.TargetPosition);

        if (oldCell == newCell) { return; }

        _occupiedCells.Remove(oldCell);
        _occupiedCells[newCell] = unit.SquadId;
    }

    public override void _Draw()
    {
        base._Draw();

        int cellSize = 64;
        int gridWidth = 20;
        int gridHeight = 20;
        Color color = new Color(1, 1, 1, 0.2f);

        for (int i = 0; i <= gridWidth; i++)
        {
            Vector2 from = new Vector2(i * cellSize, 0);
            Vector2 to = new Vector2(i * cellSize, cellSize * gridHeight);
            DrawLine(from, to, color);
        }

        for (int j = 0; j <= gridHeight; j++)
        {
            Vector2 from = new Vector2(0, j * cellSize);
            Vector2 to = new Vector2(cellSize * gridWidth, j * cellSize);
            DrawLine(from, to, color);
        }
    }

    public void ClearCellOccupation(int entityId)
    {
        var cellsForClear = new List<Vector2I>();

        foreach (var pair in _occupiedCells)
        {
            if (pair.Value == entityId)
            {
                cellsForClear.Add(pair.Key);
            }
        }
        foreach (var cell in cellsForClear)
        {
            _occupiedCells.Remove(cell);
        }

    }

    public bool IsAreaOccupied(Vector2I cell, int radius)
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                Vector2I checkCell = cell + new Vector2I(x, y);
                if (_occupiedCells.ContainsKey(checkCell)) { return true; }
            }
        }

        return false;
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        GlobalSignals.Instance.UnitSpawned -= UpdateOccupation;
    }

}