using Godot;
using InsideTheWar.Singletons;

namespace InsideTheWar.Managers;

public partial class LevelController : Node
{
    [Export] private GridManager _gridManager;
    [Export] private SpawnManager _spawnManager;

    public override void _Ready()
    {
        base._Ready();

        GlobalSignals.Instance.RequestSpawn += OnRequestSpawn;

    }

    public void OnRequestSpawn(Vector2 mousePos, string team)
    {
        var cell = _gridManager.TargetCell(mousePos);
        if (_gridManager.IsCellOccupied(cell))
        {
            GD.Print($"Cell {cell} is occupied");
            return;
        }

        if(team == "Player")
        {
            _spawnManager.SpawnSquad(mousePos, _spawnManager.UnitEngland, team);
        }
        else if(team == "AI")
        {
            _spawnManager.SpawnSquad(mousePos, _spawnManager.UnitFrance, team);
        }
        
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        GlobalSignals.Instance.RequestSpawn -= OnRequestSpawn;
    }

}
