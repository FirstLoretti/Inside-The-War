using System;
using System.Collections.Generic;
using Godot;
using InsideTheWar.Entities;
using InsideTheWar.Helpers;
using InsideTheWar.Interfaces;
using InsideTheWar.Singletons;

namespace InsideTheWar.Managers;

public partial class SpawnManager : Node2D, ISpawner
{
    [ExportGroup("Units")]
    [Export] public PackedScene UnitEngland { get; private set; }
    [Export] public PackedScene UnitFrance { get; private set; }

    [ExportGroup("EntityContainers")]
    [Export] private Node2D _playerUnits;
    [Export] private Node2D _aiUnits;

    private int _lastSquadId = 0;
    private Dictionary<StringName, Node2D> _containers;
    private Vector2I _squadColsAndRows;
    private int _formationSpacing; //! Need refactoring
    private IGrid _grid;
    private IDebug _debug;

    public override void _Ready()
    {
        _containers = new()
        {
            {Constants.PlayerUnits, _playerUnits},
            {Constants.AIUnits, _aiUnits}
        };

        AddToGroup(Constants.Debuggable);

        using (var dobby = UnitEngland.Instantiate<Unit>()) //! Need refactor
        {
            _squadColsAndRows = new Vector2I(dobby.FormationCols, dobby.FormationRows);
            _formationSpacing = dobby.FormationSpacing;
        }
    }

    public void Init(IGrid grid, IDebug debug)
    {
        _grid = grid;
        _debug = debug;
    }

    public int SpawnSquad(Vector2 spawnPosition, PackedScene unit, string unitsGroup)
    {
        int rows, cols, spacing;

        using (var dobby = unit.Instantiate<Unit>())
        {
            rows = dobby.FormationRows;
            cols = dobby.FormationCols;
            spacing = dobby.FormationSpacing;
        }

        Vector2 squadAreaSize = new(cols * spacing, rows * spacing);
        var squadArea = GameMath.CalculateAreaAroundPosition(spawnPosition, squadAreaSize, _grid.CellSize);

        if (!_grid.IsAreaFree(squadArea.centerCell, squadArea.cols, squadArea.rows))
        {
            return -1;
        }

        _lastSquadId += 1;
        var currentSquadId = _lastSquadId;

        var parentNode = _containers[unitsGroup];

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                var newUnit = unit.Instantiate<Unit>();
                newUnit.Col = col;
                newUnit.Row = row;
                newUnit.SquadId = currentSquadId;
                if (_debug != null)
                {
                    newUnit.Debug = _debug;
                }
                newUnit.AddToGroup(unitsGroup);

                var offset = GameMath.CalculateSquadOffset(col, row, newUnit.FormationCols, newUnit.FormationRows, newUnit.FormationSpacing);
                newUnit.GlobalPosition = spawnPosition + offset;

                parentNode.AddChild(newUnit);

                GlobalSignals.Instance.EmitSignal(
                    GlobalSignals.SignalName.EntitySpawned,
                    newUnit.GetInstanceId(),
                    newUnit.GlobalPosition,
                    unitsGroup);

                if (newUnit.IsInGroup(Constants.PlayerUnits)) //! Рефакторинг
                {
                    var u = (PlayerUnit)newUnit;

                    GlobalSignals.Instance.EmitSignal(
                        GlobalSignals.SignalName.EntityMoved,
                        newUnit.GetInstanceId(),
                        u.LastSignaledPosition,
                        newUnit.GlobalPosition,
                        newUnit.Stats.FogVisionDistance);

                    newUnit.EnemyUnitsGroup = Constants.AIUnits;
                }
                else
                {
                    newUnit.EnemyUnitsGroup = Constants.PlayerUnits;
                }

            }
        }

        return currentSquadId;
    }

    public override void _Draw()
    {
        if (_debug == null || !_debug.IsShowSpawnArea) { return; }

        DrawSquadOccupationArea(GetGlobalMousePosition(), _squadColsAndRows.X, _squadColsAndRows.Y, _formationSpacing, _grid.CellSize);
    }

    //! Need refactoring
    private void DrawSquadOccupationArea(Vector2 areaCenter, int cols, int rows, int spacing, int cellSize)
    {
        Vector2 squadAreaSize = new(cols * spacing, rows * spacing);
        var squadArea = GameMath.CalculateAreaAroundPosition(areaCenter, squadAreaSize, _grid.CellSize);

        Rect2 rect2 = new(squadArea.centerCell * cellSize, new Vector2(cols * cellSize, rows * cellSize));
        DrawRect(rect2, Colors.Cyan with { A = 0.3f });
    }

}
