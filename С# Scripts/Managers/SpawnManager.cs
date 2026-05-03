using System;
using System.Collections.Generic;
using Godot;
using InsideTheWar.Entities;
using InsideTheWar.Helpers;
using InsideTheWar.Singletons;

namespace InsideTheWar.Managers;

public partial class SpawnManager : Node
{
    [ExportGroup("Units")]
    [Export] public PackedScene UnitEngland { get; private set; }
    [Export] public PackedScene UnitFrance { get; private set; }

    [ExportGroup("EntityContainers")]
    [Export] private Node2D _playerUnits;
    [Export] private Node2D _aiUnits;

    private int _lastSquadId = 0;
    private Dictionary<String, Node2D> _containers;

    public override void _Ready()
    {
        base._Ready();

        _containers = new()
        {
            {"PlayerUnits", _playerUnits},
            {"AIUnits", _aiUnits}
        };

    }

    public int SpawnSquad(Vector2 spawnPosition, PackedScene unit, string team)
    {
        _lastSquadId += 1;
        var currentSquadId = _lastSquadId;

        var parentNode = _containers[team + "Units"];

        int rows, cols, spacing;

        using (var dobby = unit.Instantiate<Unit>())
        {
            rows = dobby.FormationRows;
            cols = dobby.FormationCols;
            spacing = dobby.FormationSpacing;
        }

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                var newUnit = unit.Instantiate<Unit>();
                newUnit.Col = col;
                newUnit.Row = row;
                newUnit.SquadId = currentSquadId;
                newUnit.AddToGroup(team + "Units");

                var offset = GameMath.CalculateSquadOffset(col, row, newUnit.FormationCols, newUnit.FormationRows, newUnit.FormationSpacing);
                newUnit.GlobalPosition = spawnPosition + offset;

                parentNode.AddChild(newUnit);

                GlobalSignals.Instance.EmitSignal(
                    GlobalSignals.SignalName.EntitySpawned,
                    newUnit.GetInstanceId(),
                    newUnit.GlobalPosition);

                if (newUnit.IsInGroup("PlayerUnits")) //! Рефакторинг
                {
                    GlobalSignals.Instance.EmitSignal(
                        GlobalSignals.SignalName.EntityMoved,
                        newUnit.GetInstanceId(),
                        newUnit.LastSignaledPos,
                        newUnit.GlobalPosition,
                        newUnit.Stats.FogVisionDistance);
                }
            }
        }

        return currentSquadId;
    }

}
