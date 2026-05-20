using Godot;
using InsideTheWar.Helpers;

namespace InsideTheWar.Entities.Components;

public partial class MovementComponent : Node
{
    public Vector2 TargetPosition { get; private set; }
    public Vector2 LookDirection { get; private set; }

    private CharacterBody2D _body;
    private Sprite2D _sprite;
    private float _arrivalDistance;
    private float _minSpeed;
    private float _maxSpeed;

    public void Initialize(
        CharacterBody2D body,
        Sprite2D sprite2D,
        float minSpeed,
        float maxSpeed,
        float arrivalDistance
    )
    {
        _body = body;
        _sprite = sprite2D;
        _minSpeed = minSpeed;
        _maxSpeed = maxSpeed;
        _arrivalDistance = arrivalDistance;
        TargetPosition = _body.GlobalPosition;
    }

    public void MoveTo(Vector2 targetPosition)
    {
        TargetPosition = targetPosition;

        var targetDirection = _body.GlobalPosition.DirectionTo(targetPosition);
        var distanceToTarget = _body.GlobalPosition.DistanceTo(targetPosition);
        var currentSpeed = GameMath.CalculateSpeedInThisFrame(
            _minSpeed,
            _maxSpeed,
            distanceToTarget,
            _arrivalDistance);
        _body.Velocity = targetDirection * currentSpeed;
        _body.MoveAndSlide();

        LookAt(targetDirection);
    }

    public void Stop()
    {
        _body.GlobalPosition = TargetPosition;
        _body.Velocity = Vector2.Zero;
    }

    public void LookAt(Vector2 targetDirection)
    {
        if (targetDirection != Vector2.Zero)
        {
            LookDirection = targetDirection;
        }
        _sprite.FlipH = LookDirection.X < Constants.Zero;
    }
}
