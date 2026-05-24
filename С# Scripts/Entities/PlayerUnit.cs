using Godot;
using InsideTheWar.Data;
using InsideTheWar.Singletons;

namespace InsideTheWar.Entities;

public partial class PlayerUnit : Unit
{
    public bool IsSelected = false;

    public Vector2 LastSignaledPosition { get; set; }

    public override void _Ready()
    {
        base._Ready();
        LastSignaledPosition = GlobalPosition;
    }

    protected override void MoveTo(Vector2 targetPosition)
    {
        base.MoveTo(targetPosition);
        CheckFogUpdate();
    }

    private void CheckFogUpdate()
    {
        var data = (PlayerUnitData)Data;
        if (GlobalPosition.DistanceTo(LastSignaledPosition) > _updateFogTriggerDistance)
        {
            GlobalSignals.Instance.EmitSignal(GlobalSignals.SignalName.EntityMoved,
            GetInstanceId(), LastSignaledPosition, GlobalPosition, data.FogVisionDistance);

            LastSignaledPosition = GlobalPosition;
        }
    }
}
