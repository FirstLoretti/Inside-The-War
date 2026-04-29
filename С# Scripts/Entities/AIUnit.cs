using Godot;
using InsideTheWar.Data;
using InsideTheWar.Helpers;
using System;

namespace InsideTheWar.Entities;

public partial class AIUnit : Unit
{
    [Export] private float _checkEnemiesInVisionTimer = 0.25f;
    [Export] private float _movementRadiusMin = 50.0f;
    [Export] private float _movementRadiusMax = 200.0f;

    private float _currentCheckTimer;

    public float MovementRadiusMin => _movementRadiusMin;
    public float MovementRadiusMax => _movementRadiusMax;

    public float MinWaitingTime { get; private set; }
    public float MaxWaitingTime { get; private set; }
    public float RandomWaitingTime { get; set; }

    public AISquad MySquad { get; set; }

    public event Action<AIUnit> ReadyToAct;
    public event Action<Node2D> EnemySpotted;

    private AIUnitData AIStats => (AIUnitData)Stats;

    public override void _Ready()
    {
        base._Ready();

        MinWaitingTime = AIStats.MinWaitingTime;
        MaxWaitingTime = AIStats.MaxWaitingTime;

        _currentCheckTimer = _checkEnemiesInVisionTimer;
    }
    //! Зарефакторить
    protected override void ProcessMovement(float delta)
    {
        _currentCheckTimer -= delta;
        if (_currentCheckTimer <= 0.0f)
        {
            CheckForEnemies();
            _currentCheckTimer = _checkEnemiesInVisionTimer;
        }

        if (CurrentState == UnitStates.Waiting) { return; }

        if (CurrentState == UnitStates.Idle)
        {
            RandomWaitingTime -= delta;

            if (RandomWaitingTime <= 0.0f)
            {
                CurrentState = UnitStates.Waiting;
                ReadyToAct?.Invoke(this);
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
            (_stats.MaxSpeed, _stats.MinSpeed, distanceTo, _arrivalDistance);

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

    private void CheckForEnemies()
    {
        var overlappingAreas = _visionArea.GetOverlappingAreas();

        foreach (var area in overlappingAreas)
        {   
            Node2D entity = (Node2D)area.GetParent();

            if (entity.IsInGroup("PlayerUnits"))
            {
                EnemySpotted?.Invoke(entity);
            }

            break;
        }
    }

}
