using Godot;

namespace InsideTheWar.Singletons;

public partial class Signals : Node
{
    [Signal]
    public delegate void PlayerLeaderPositionChangedEventHandler(Vector2 newPosition, int vision, int squadID);
    [Signal]
    public delegate void EntityMovedEventHandler(int id, Vector2I targetCell, float vision);

    public static Signals Instance { get; private set; }

    public override void _Ready()
    {
        base._Ready();
        Instance = this;
    }

}
