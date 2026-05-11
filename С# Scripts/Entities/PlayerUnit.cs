using Godot;
using InsideTheWar.Singletons;

namespace InsideTheWar.Entities;

public partial class PlayerUnit : Unit
{
    [Export]
    
    public bool IsSelected = false;

    public Vector2 LastSignaledPosition { get; set; }

    public override void _Ready()
    {
        base._Ready();
        LastSignaledPosition = GlobalPosition;
    }

    public override void MoveTo(Vector2 targetPosition)
    {
        base.MoveTo(targetPosition);
        CheckFogUpdate();
    }

    private void CheckFogUpdate()
    {
        if (GlobalPosition.DistanceTo(LastSignaledPosition) > _updateFogTriggerDistance)
        {
            GlobalSignals.Instance.EmitSignal(GlobalSignals.SignalName.EntityMoved,
            GetInstanceId(), LastSignaledPosition, GlobalPosition, Stats.FogVisionDistance);

            LastSignaledPosition = GlobalPosition;
        }
    }
}
