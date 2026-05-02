using Godot;
using InsideTheWar.Data;
using InsideTheWar.Helpers;
using System;
using System.Linq;

namespace InsideTheWar.Entities;

public partial class AIUnit : Unit
{
    [Export] private float _checkEnemiesInVisionTimer = 0.25f;
    [Export] private float _checkEnemiesInChargeTimer = 0.1f;
    [Export] private float _movementRadiusMin = 50.0f;
    [Export] private float _movementRadiusMax = 200.0f;
    public float MovementRadiusMin => _movementRadiusMin;
    public float MovementRadiusMax => _movementRadiusMax;

    public float MinIdleTime { get; private set; }
    public float MaxIdleTime { get; private set; }
    public float RandomIdleTime { get; set; }
    public AISquad MySquad { get; set; }
    // public new UnitStates CurrentState
    // {
    //     get => _currentStateInternal;
    //     set
    //     {
    //         if(_currentStateInternal != value)
    //         {
    //             GD.Print($"[Unit] {Name}: {_currentStateInternal} -> {value}");
    //             //GD.Print(System.Environment.StackTrace);

    //             _currentStateInternal = value;
    //         }
    //     }
    // }

    public event Action<AIUnit> ReadyToAct;
    public event Action<Node2D> EnemySpotted;

    private float _currentVisionTimer;
    private float _currentChargeVisionTimer;
    private Node2D _currentAttackTarget;
    private AIUnitData AIStats => (AIUnitData)Stats;
    //private UnitStates _currentStateInternal;

    public override void _Ready()
    {
        base._Ready();

        MinIdleTime = AIStats.MaxIdleTime;
        MaxIdleTime = AIStats.MaxIdleTime;

        _currentVisionTimer = 0.0f;
        _currentChargeVisionTimer = 0.0f;
    }

    public override void _Process(double delta)
    {
        UpdateVision((float)delta);

        if (CurrentState == UnitStates.Idle)
        {
            UpdateIdleTimer((float)delta);
        }

        base._Process(delta);
    }

    protected override void ProcessMovement(float delta)
    {
        if (CurrentState != UnitStates.Moving && CurrentState != UnitStates.Charging) { return; }

        if (CurrentState == UnitStates.Charging)
        {
            UpdateChargeTimer(delta);

            if (CurrentState == UnitStates.Attacking || CurrentState == UnitStates.BattleReady)
            {
                Velocity = Vector2.Zero;

                if (CurrentState == UnitStates.BattleReady)
                {
                    _animationPlayer.Play(IdleAnim);
                }

                return;
            }
        }

        float distanceToTarget = GlobalPosition.DistanceTo(TargetPosition);

        if (distanceToTarget <= _stoppingDistance)
        {
            HandleArrival();
        }
        else
        {
            MoveTowardsTarget(distanceToTarget);
        }
    }

    private void HandleArrival()
    {
        StopAtTarget();

        if (CurrentState == UnitStates.Charging)
        {
            EngageTarget();
        }
        else
        {
            CurrentState = UnitStates.Idle;
            _animationPlayer.Play(IdleAnim);
        }
    }

    private void MoveTowardsTarget(float distanceTo)
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

    private void CheckForEnemies()
    {
        var entities = _visionArea.GetOverlappingBodies();

        foreach (var entity in entities)
        {
            if (entity.IsInGroup("PlayerUnits"))
            {
                EnemySpotted?.Invoke(entity);
            }

            break;
        }
    }

    private void EngageTarget() //! Дубляж с CheckForEnemies
    {
        var targets = _attackDistanceArea.GetOverlappingBodies()
        .OfType<Unit>()
        .Where(t => t.IsInGroup("PlayerUnits")) //! Сделать общую константу PlayerGroup
        .OrderBy(t => t.GlobalPosition.DistanceSquaredTo(GlobalPosition));

        foreach (var targetUnit in targets)
        {
            if (targetUnit.UnitsAttackingMe.Count < Unit.MaxUnitsAttackers)
            {
                targetUnit.UnitsAttackingMe.Add(this);
                _currentAttackTarget = targetUnit;
                CurrentState = UnitStates.Attacking;

                _animationPlayer.Play(AttackAnim);
                _sprite2D.FlipH = GlobalPosition.DirectionTo(_currentAttackTarget.GlobalPosition).X < 0.0f;

                return;
            }
        }

        CurrentState = UnitStates.BattleReady;
    }

    private void UpdateVision(float delta)
    {
        if (CurrentState == UnitStates.Attacking) { return; }

        _currentVisionTimer -= delta;
        if (_currentVisionTimer <= 0.0f)
        {
            CheckForEnemies();
            _currentVisionTimer = _checkEnemiesInVisionTimer;
        }
    }

    private void UpdateIdleTimer(float delta)
    {
        RandomIdleTime -= delta;

        if (RandomIdleTime <= 0.0f)
        {
            CurrentState = UnitStates.WaitingOrder;
            ReadyToAct?.Invoke(this);
        }
    }

    private void UpdateChargeTimer(float delta)
    {
        _currentChargeVisionTimer -= delta;

        if (_currentChargeVisionTimer <= 0.0f)
        {
            ProcessCharge();
            _currentChargeVisionTimer = _checkEnemiesInChargeTimer;
        }
    }

    private void ProcessCharge()
    {
        if (GlobalPosition.DistanceTo(TargetPosition) <= AttackDistanceRadius)
        {
            EngageTarget();
        }
    }

    private void StopAtTarget()
    {
        GlobalPosition = TargetPosition;
        Velocity = Vector2.Zero;
    }

    public void ReportReady()
    {
        ReadyToAct?.Invoke(this);
    }

    public void MoveTo(Vector2 targetPosition, float idleTimeAfterReach)
    {
        TargetPosition = targetPosition;
        RandomIdleTime = idleTimeAfterReach;

        CurrentState = UnitStates.Moving;
        _animationPlayer.Play(RunAnim);
    }

    public void Charge(Vector2 targetPosition)
    {
        TargetPosition = targetPosition;
        CurrentState = UnitStates.Charging;
    }

}
