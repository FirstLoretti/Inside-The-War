using Godot;
using InsideTheWar.Data;
using InsideTheWar.Helpers;
using InsideTheWar.Singletons;

namespace InsideTheWar.Entities;

public partial class AIUnit : Unit
{
    [Export] private float _movementRadiusMin = 50.0f;
    [Export] private float _movementRadiusMax = 200.0f;

    public float MovementRadiusMin => _movementRadiusMin;
    public float MovementRadiusMax => _movementRadiusMax;

    public float MinWaitingTime { get; private set; }
    public float MaxWaitingTime { get; private set; }
    public float RandomWaitingTime { get; set; }

    private AIUnitData AIStats => (AIUnitData)Stats;

    public override void _Ready()
    {
        base._Ready();

        MinWaitingTime = AIStats.MinWaitingTime;
        MaxWaitingTime = AIStats.MaxWaitingTime;
    }

    protected override void ProcessMovement(float delta)
    {
        if (CurrentState == UnitStates.Waiting) { return; }

        if (CurrentState == UnitStates.Idle)
        {
            RandomWaitingTime -= delta;

            if (RandomWaitingTime <= 0.0f)
            {
                CurrentState = UnitStates.Waiting;

                GlobalSignals.Instance.EmitSignal(GlobalSignals.SignalName.AIUnitReady, this);
            }

            return;
        }

        // CurrentState Moving
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

    public void MoveTo(Vector2 targetPosition, float waitingTimeAfterReach)
    {
        TargetPosition = targetPosition;
        RandomWaitingTime = waitingTimeAfterReach;

        CurrentState = UnitStates.Moving;
        _animationPlayer.Play(RunAnim);
    }
}
