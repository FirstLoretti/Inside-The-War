using Godot;

namespace InsideTheWar.Singletons;

public partial class GlobalDebugManager : Node
{
    public GlobalDebugManager Instance { get; private set; }
    public static bool IsEnabled { get; private set; }

    public override void _Ready()
    {
        Instance = this;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("debug_toggle"))
        {
            IsEnabled = !IsEnabled;

            GetTree().CallGroup("Debuggable", "queue_redraw");
        }
    }

}
