using System.Collections.Generic;
using Godot;
using InsideTheWar.Singletons;

namespace InsideTheWar.Managers;

public partial class GridManager : Node2D
{
    [Export] private TileMapLayer _mainMap;
    [Export] private int _cellSize = 64;

    private Dictionary<Vector2I, ulong> _occupiedCells = new();

    public Vector2 TargetPixels(Vector2I targetCell) => _mainMap.MapToLocal(targetCell);

    public Vector2I TargetCell(Vector2 targetPixels) => _mainMap.LocalToMap(targetPixels);

    public bool IsCellOccupied(Vector2I cellCoords) => _occupiedCells.ContainsKey(cellCoords);

    // public void SetOccupied(Vector2 pos, ulong entityId) => _occupiedCells[TargetCell(pos)] = entityId;


    public override void _Ready()
    {
        base._Ready();

        GlobalSignals.Instance.EntitySpawned += OnEntitySpawned;
        GlobalSignals.Instance.EntityMoved += OnEntityMoved;
    }

    private void OnEntitySpawned(ulong id, Vector2 currentPos)
    {
        var spawnCell = TargetCell(currentPos);
        _occupiedCells[spawnCell] = id;
        QueueRedraw();
    }

    private void OnEntityMoved(ulong id, Vector2 oldPos, Vector2 currentPos, int vision)
    {
        Vector2I oldCell = TargetCell(oldPos);
        Vector2I newCell = TargetCell(currentPos);

        if (oldCell != newCell)
        {
            if (_occupiedCells.TryGetValue(oldCell, out ulong existingID) && existingID == id)
            {
                _occupiedCells.Remove(oldCell);
            }

            _occupiedCells[newCell] = id;
            QueueRedraw();
        }
    }

    public override void _Draw()
    {
        base._Draw();
        DrawGrid();
        DrawOccupationCells();
    }
    /*
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
  */
    private void DrawGrid()
    {
        int gridWidth = 20;
        int gridHeight = 20;
        Color color = new Color(1, 1, 1, 0.2f);

        for (int i = 0; i <= gridWidth; i++)
        {
            Vector2 from = new Vector2(i * _cellSize, 0);
            Vector2 to = new Vector2(i * _cellSize, _cellSize * gridHeight);
            DrawLine(from, to, color);
        }

        for (int j = 0; j <= gridHeight; j++)
        {
            Vector2 from = new Vector2(0, j * _cellSize);
            Vector2 to = new Vector2(_cellSize * gridWidth, j * _cellSize);
            DrawLine(from, to, color);
        }
    }

    private void DrawOccupationCells()
    {
        foreach (var cell in _occupiedCells.Keys)
        {
            var rect = new Rect2(cell * _cellSize, new Vector2(_cellSize, _cellSize));
            DrawRect(rect, new Color(1, 0, 0, 0.3f));
        }
    }
    public override void _ExitTree()
    {
        base._ExitTree();

        GlobalSignals.Instance.EntitySpawned -= OnEntitySpawned;
        GlobalSignals.Instance.EntityMoved -= OnEntityMoved;
    }

}