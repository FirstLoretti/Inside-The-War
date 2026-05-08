using Godot;
using InsideTheWar.Helpers;
using InsideTheWar.Singletons;

namespace InsideTheWar.Managers;

public partial class LevelController : Node
{
    [Export] private GridManager _gridManager;
    [Export] private SpawnManager _spawnManager;
    [Export] private DebugManager _debugManager;
    [Export] private AIUnitManager _aiUnitmanager;

    public override void _Ready()
    {
        GlobalSignals.Instance.RequestSpawn += OnRequestSpawn;

        _spawnManager.Init(_gridManager, _debugManager);//! Check Init for null
        _debugManager.Init(_spawnManager);
        _gridManager.Init(_debugManager);
        _aiUnitmanager.Init(_debugManager);
    }

    public void OnRequestSpawn(Vector2 mousePos, StringName unitsGroup)
    {
        var cell = _gridManager.TargetCell(mousePos);
        if (_gridManager.IsCellOccupied(cell))
        {
            GD.Print($"Cell {cell} is occupied");
            return;
        }

        if(unitsGroup == Constants.PlayerUnits)
        {
            _spawnManager.SpawnSquad(mousePos, _spawnManager.UnitEngland, Constants.PlayerUnits);
        }
        else if(unitsGroup == Constants.AIUnits)
        {
            _spawnManager.SpawnSquad(mousePos, _spawnManager.UnitFrance, Constants.AIUnits);
        }
        
    }

    public override void _ExitTree()
    {
        GlobalSignals.Instance.RequestSpawn -= OnRequestSpawn;
    }

}
