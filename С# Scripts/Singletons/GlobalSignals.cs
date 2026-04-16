using Godot;
using InsideTheWar.Entities;

namespace InsideTheWar.Singletons;

public partial class GlobalSignals : Node
{
    [Signal]
    public delegate void PlayerLeaderPositionChangedEventHandler(Vector2 newPosition, int vision, int squadID);
    [Signal]
    public delegate void EntityMovedEventHandler(int id, Vector2 oldPos, Vector2 currentPos, int vision);
    [Signal]
    public delegate void UnitSpawnedEventHandler(Unit unit);
    [Signal]
    public delegate void RequestSpawnEventHandler(Vector2 mousPos, string team);

    public static GlobalSignals Instance { get; private set; }

    public override void _Ready()
    {
        base._Ready();
        Instance = this;
    }

}
