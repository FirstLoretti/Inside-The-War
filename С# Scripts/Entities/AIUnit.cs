using Godot;
using InsideTheWar.Data;
using InsideTheWar.Helpers;
using InsideTheWar.Interfaces;
using System;
using System.Linq;

namespace InsideTheWar.Entities;

public partial class AIUnit : Unit
{
    [Export] private float _movementRadiusMin = 50.0f;
    [Export] private float _movementRadiusMax = 200.0f;

    public float MovementRadiusMin => _movementRadiusMin;
    public float MovementRadiusMax => _movementRadiusMax;

    public float MinIdleTime { get; private set; }
    public float MaxIdleTime { get; private set; }
    public float RandomIdleTime { get; set; }

    public event Action<AIUnit> ReadyToAct;
    public event Action<Node2D> EnemySpotted;

    private float _checkForEnemiesTimer;

    private AIUnitData AIData => (AIUnitData)Data;

    public override void _Ready()
    {
        base._Ready();
        MinIdleTime = AIData.MaxIdleTime;
        MaxIdleTime = AIData.MaxIdleTime;
    }

    // public override void _PhysicsProcess(double delta)
    // {
    //     var enemies = _attackDistance.GetOverlappingBodies()
    //         .OfType<IDamageable>()
    //         .Where(e => e is Node2D node && node.IsInGroup(EnemyGroup))
    //         .OrderBy(e => e.GlobalPosition.DistanceSquaredTo(GlobalPosition))
    //         .ToList();
    //     //GD.Print($"[ЖИВОЙ ЮНИТ] ID: {GetInstanceId()}, Слышит тел: {enemies.Count}");
    //     if (CurrentState != UnitStates.Attacking && CurrentState != UnitStates.Dead)
    //     {
    //         TickCheckForEnemiesInFOV((float)delta);
    //     }

    //     if (CurrentState == UnitStates.Idle)
    //     {
    //         TickIdling((float)delta);
    //     }

    //     base._PhysicsProcess(delta);
    // }

    private void CheckForEnemyInFOV()
    {
        var enemy = _visionDistance.GetOverlappingBodies()
            .FirstOrDefault(e => e.IsInGroup(EnemyGroup));

        if (enemy != null)
        {
            EnemySpotted?.Invoke(enemy);
        }
    }

    private void TickCheckForEnemiesInFOV(float delta)
    {
        _checkForEnemiesTimer -= delta;
        if (_checkForEnemiesTimer <= Constants.Zero)
        {
            CheckForEnemyInFOV();
            _checkForEnemiesTimer = _timer;
        }
    }

    private void TickIdling(float delta)
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
