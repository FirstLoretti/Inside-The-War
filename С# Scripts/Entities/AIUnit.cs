using Godot;
using InsideTheWar.Data;
using InsideTheWar.Helpers;

namespace InsideTheWar.Entities;

public partial class AIUnit : Unit
{
    [Export] private float _movementRadiusMin = 50.0f;
    [Export] private float _movementRadiusMax = 200.0f;

    private float _minWaitingTime;
    private float _maxWaitingTime;
    private float _randomWaitingTime;

    private AIUnitData AIStats => (AIUnitData)Stats;

    public override void _Ready()
    {
        base._Ready();

        _minWaitingTime = AIStats.MinWaitingTime;
        _maxWaitingTime = AIStats.MaxWaitingTime;
        _randomWaitingTime = GameMath.GetRandomNumber(_minWaitingTime, _maxWaitingTime);
    }

    protected void SetRandomDestination()
    {
        TargetPosition = GameMath.GetRandomPointInCircle(GlobalPosition, _movementRadiusMin, _movementRadiusMax);
    }

    protected override void ProcessMovement(float delta)
    {
        if (CurrentState == UnitStates.Idle)
        {
            _randomWaitingTime -= delta;

            if (_randomWaitingTime <= 0.0f)
            {
                _randomWaitingTime = GameMath.GetRandomNumber(_minWaitingTime, _maxWaitingTime);
                SetRandomDestination();
                CurrentState = UnitStates.Moving;
                _animationPlayer.Play(RunAnim);
            }

            return;
        }

        float distanceTo = GlobalPosition.DistanceTo(TargetPosition);

        if (distanceTo <= _stoppingDistance)
        {
            GlobalPosition = TargetPosition;

            Velocity = Vector2.Zero;
            CurrentState = UnitStates.Idle;
            _animationPlayer.Play(IdleAnim);
        }
        else
        {
            var direction = GlobalPosition.DirectionTo(TargetPosition);
            var speedInThisFrame = GameMath.CalculateSpeedInThisFrame
            (Stats.MaxSpeed, Stats.MinSpeed, distanceTo, _arrivalDistance);

            //var avoidance = GameMath.CalculateAvoidance(_avoidanceArea, this);
            //Vector2 combinedDirection = (direction + avoidance * Stats.AvoidanceWeight).Normalized();

            Velocity = direction * speedInThisFrame;
            MoveAndSlide();

            _sprite2D.FlipH = direction.X < 0.0f;
        }
    }

}
