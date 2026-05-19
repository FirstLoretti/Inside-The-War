using Godot;
using InsideTheWar.Entities;

namespace InsideTheWar.Helpers;

public partial class UnitDebuger : Node2D
{
    private Unit _unit;

    public void Initialize(Unit unit)
    {
        _unit = unit;
        TopLevel = true;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!IsInstanceValid(_unit) || _unit.CurrentState == UnitStates.Dead)
        {
            QueueFree();
            return;
        }
        GlobalPosition = _unit.GlobalPosition;
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (IsInstanceValid(_unit) && _unit.Debug.IsEnabled)
        {
            var movementLineColor = _unit.CurrentState == UnitStates.Moving ? Colors.Green : Colors.Blue;
            var direction = (_unit.MovementTargetPosition - GlobalPosition).Normalized();
            var rayDistance = direction * (_unit.FormationData.Spacing * Unit.CheckAllyRayMultiplicator);

            DrawCircle(Vector2.Zero, _unit.Data.AttackDistance, Colors.Orange with { A = 0.5f });
            DrawLine(Vector2.Zero, ToLocal(_unit.MovementTargetPosition), movementLineColor, 4.0f);
            DrawLine(Vector2.Zero, rayDistance, Colors.Black with { A = 0.3f }, 16.0f);
        }
    }
}