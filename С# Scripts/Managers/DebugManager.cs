using Godot;
using InsideTheWar.Interfaces;

namespace InsideTheWar.Managers;

public partial class DebugManager : Node, IDebug
{
    private bool _isEnabled = true;
    private bool _isShowSpawnArea;
    public bool IsEnabled => _isEnabled;
    public bool IsShowSpawnArea => _isShowSpawnArea;

    private ISpawner _spawner;

    public void Init(ISpawner spawner)
    {
        _spawner = spawner;
    }

    public override void _Ready()
    {
        base._Ready();
    }


    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("debug_toggle"))
        {
            _isEnabled = !_isEnabled;
            GetTree().CallGroup("Debuggable", "queue_redraw");
            return;
        }

        if (!_isEnabled) { return; }
        if (_spawner == null) { return; }
        
        ShowSpawnArea(@event);
    }

    private void ShowSpawnArea(InputEvent @event)
    {
        var isKeyboardEvent = @event.IsActionPressed("draw_spawn_area") || @event.IsActionReleased("draw_spawn_area");
        var isMouseEvent = _isShowSpawnArea && @event is InputEventMouseMotion;

        if (isKeyboardEvent || isMouseEvent)
        {
            if (isKeyboardEvent)
            {
                _isShowSpawnArea = Input.IsActionPressed("draw_spawn_area");
                _spawner.QueueRedraw();
            }
            else if (isMouseEvent)
            {
                _spawner.QueueRedraw();
            }
        }
    }
}
