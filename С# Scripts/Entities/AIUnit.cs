using Godot;
using InsideTheWar.Data;
using InsideTheWar.Helpers;
using System;

namespace InsideTheWar.Entities;

public partial class AIUnit : Unit
{
    [Export] private float _checkEnemiesInVisionTimer = 0.1f;
    [Export] private float _movementRadiusMin = 50.0f;
    [Export] private float _movementRadiusMax = 200.0f;
    public float MovementRadiusMin => _movementRadiusMin;
    public float MovementRadiusMax => _movementRadiusMax;

    public float MinIdleTime { get; private set; }
    public float MaxIdleTime { get; private set; }
    public float RandomIdleTime { get; set; }
    public AISquad MySquad { get; set; }

    #region // DebugCurrentState
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
    //private UnitStates _currentStateInternal;
    #endregion

    public event Action<AIUnit> ReadyToAct;
    public event Action<Node2D> EnemySpotted;

    private float _checkForEnemiesTimer;

    private AIUnitData AIStats => (AIUnitData)Stats;

    public override void _Ready()
    {
        base._Ready();
        MinIdleTime = AIStats.MaxIdleTime;
        MaxIdleTime = AIStats.MaxIdleTime;
        _checkForEnemiesTimer = Constants.Zero;
    }

    public override void _Process(double delta)
    {
        UpdateCheckForEnemiesTimer((float)delta);

        if (CurrentState == UnitStates.Idle)
        {
            UpdateIdleTimer((float)delta);
        }

        base._Process(delta);
    }

    protected override void UpdateMovement(float delta, Vector2 targetPosition)
    {
        if (CurrentState == UnitStates.Charging)
        {
            UpdateCheckForEnemiesTimer(delta);;
        }

        base.UpdateMovement(delta, TargetPosition);
    }

    private void CheckForEnemies()
    {
        var entities = _visionArea.GetOverlappingBodies();

        foreach (var entity in entities)
        {
            if (entity.IsInGroup(EnemyUnitsGroup))
            {
                EnemySpotted?.Invoke(entity);
            }
            break;
        }
    }

    private void UpdateCheckForEnemiesTimer(float delta)
    {
        if (CurrentState == UnitStates.Attacking) { return; }

        _checkForEnemiesTimer -= delta;
        if (_checkForEnemiesTimer <= Constants.Zero)
        {
            CheckForEnemies();
            _checkForEnemiesTimer = _checkEnemiesInVisionTimer;
        }
    }

    private void UpdateIdleTimer(float delta)
    {
        RandomIdleTime -= delta;
        if (RandomIdleTime <= Constants.Zero)
        {
            CurrentState = UnitStates.WaitingOrder;
            ReadyToAct?.Invoke(this);
        }
    }

    public void ReportReady()
    {
        ReadyToAct?.Invoke(this);
    }

    public void MoveTo(Vector2 targetPosition, float idleTimeAfterReach)
    {
        base.MoveTo(targetPosition);
        RandomIdleTime = idleTimeAfterReach;
    }
}
