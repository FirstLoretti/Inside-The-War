using Godot;

namespace InsideTheWar.Singletons;

public partial class GlobalSignals : Node
{
    //[Signal]
    //public delegate void PlayerLeaderPositionChangedEventHandler(Vector2 newPosition, int vision, int squadID);
    [Signal]
    public delegate void EntityMovedEventHandler(ulong id, Vector2 oldPosition, Vector2 currentPosition, int vision);
    [Signal]
    public delegate void EnitySpawnedEventHandler(ulong id, Vector2 currentPosition);
    [Signal]
    public delegate void RequestSpawnEventHandler(Vector2 mousPosition, string team);

    public static GlobalSignals Instance { get; private set; }

    public override void _Ready()
    {
        base._Ready();
        Instance = this;
    }

}
