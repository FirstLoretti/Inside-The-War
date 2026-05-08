using Godot;
using InsideTheWar.Helpers;
using InsideTheWar.Data;
using System.Collections.Generic;
using InsideTheWar.Interfaces;
using System.Linq;
using System;

namespace InsideTheWar.Entities;

public partial class Unit : CharacterBody2D, IUnit
{
    [ExportGroup("Stats")]
    [Export] protected BaseUnitData _stats;
    public BaseUnitData Stats => _stats;

    [ExportGroup("FormationSettings")]
    [Export] protected int _formationCols = 3;
    [Export] protected int _formationRows = 3;
    [Export] protected int _formationSpacing = 20;
    public int FormationCols => _formationCols;
    public int FormationRows => _formationRows;
    public int FormationSpacing => _formationSpacing;

    [ExportGroup("Dependencies")]
    [Export] protected Area2D _visionArea;
    [Export] protected Area2D _attackDistanceArea;
    [Export] protected AnimationPlayer _animationPlayer;
    [Export] protected Sprite2D _sprite2D;

    #region // IfAvoidanceOn
    //[Export] protected Area2D _avoidanceArea;
    #endregion

    public UnitStates CurrentState { get; protected set; }
    public List<Unit> UnitAttackers = [];
    public Vector2 TargetPosition { get; set; }
    public int SquadId { get; set; }
    public int Row { get; set; }
    public int Col { get; set; }
    public ulong Id { get; set; }
    public bool IsMoving => GlobalPosition.DistanceTo(TargetPosition) > _stoppingDistance;
    public IDebug Debug { get; set; }
    public const int MaxAttackers = 1;
    public StringName EnemyUnitsGroup { get; set; }

    protected static readonly StringName RunAnim = "Run";
    protected static readonly StringName IdleAnim = "Idle";
    protected static readonly StringName AttackAnim = "Attack";
    protected const float _stoppingDistance = 5.0f;
    protected const float _stoppingDistanceSqr = _stoppingDistance * _stoppingDistance;
    protected const float _arrivalDistance = 50.0f;
    protected const float _updateFogTriggerDistance = 32.0f;
    protected Unit _currentAttackTarget;

    public override void _Ready()
    {
        TargetPosition = GlobalPosition;
        Id = GetInstanceId();
        CurrentState = UnitStates.WaitingOrder;
        AddToGroup(Constants.Debuggable);
        _animationPlayer.Play(IdleAnim);
    }

    public override void _Process(double delta)
    {
        if (Debug.IsEnabled)
        {
            QueueRedraw();
        }

        if (CurrentState == UnitStates.Attacking || CurrentState == UnitStates.BattleReady)
        {
            Velocity = Vector2.Zero;

            if (CurrentState == UnitStates.BattleReady)
            {
                _animationPlayer.Play(IdleAnim);
            }

            return;
        }

        if (CurrentState == UnitStates.Idle || CurrentState == UnitStates.WaitingOrder) { return; }

        UpdateMovement((float)delta, TargetPosition);
    }

    public virtual void MoveTo(Vector2 targetPosition) //! Refactoring
    {
        if (CurrentState != UnitStates.Charging)
        {
            CurrentState = UnitStates.Moving;
        }
        TargetPosition = targetPosition;

        var direction = GlobalPosition.DirectionTo(targetPosition);
        var speed = GameMath.CalculateSpeedInThisFrame(
            _stats.MaxSpeed,
            _stats.MinSpeed,
            GlobalPosition.DistanceTo(targetPosition),
            _arrivalDistance);

        #region // IfAvoidanceOn
        //var avoidance = GameMath.CalculateAvoidance(_avoidanceArea, this);
        //Vector2 combinedDirection = (direction + avoidance * Stats.AvoidanceWeight).Normalized();
        #endregion

        Velocity = direction * speed;
        MoveAndSlide();

        _animationPlayer.Play(RunAnim);
        _sprite2D.FlipH = direction.X < Constants.Zero;
    }

    protected virtual void UpdateMovement(float delta, Vector2 targetPosition)
    {
        var distanceToTargetSqr = GlobalPosition.DistanceSquaredTo(targetPosition);

        if (distanceToTargetSqr <= _stoppingDistanceSqr)
        {
            OnReachDestination();
        }
        else
        {
            MoveTo(targetPosition);
        }
    }

    // protected virtual void UpdateMovement(float delta)
    // {

    //     float distanceTo = GlobalPosition.DistanceTo(TargetPosition);

    //     if (distanceTo <= _stoppingDistance)
    //     {
    //         GlobalPosition = TargetPosition;

    //         if (GlobalPosition != LastSignaledPosition && this.IsInGroup("PlayerUnits")) //! Рефакторинг
    //         {
    //             GlobalSignals.Instance.EmitSignal(GlobalSignals.SignalName.EntityMoved,
    //             GetInstanceId(), LastSignaledPosition, GlobalPosition, Stats.FogVisionDistance);

    //             LastSignaledPosition = GlobalPosition;
    //         }

    //         Velocity = Vector2.Zero;
    //         CurrentState = UnitStates.Idle;
    //         _animationPlayer.Play(IdleAnim);
    //     }
    //     else
    //     {
    //         var direction = GlobalPosition.DirectionTo(TargetPosition);
    //         var speedInThisFrame = GameMath.CalculateSpeedInThisFrame
    //         (Stats.MaxSpeed, Stats.MinSpeed, distanceTo, _arrivalDistance);

    //         //var avoidance = GameMath.CalculateAvoidance(_avoidanceArea, this);
    //         //Vector2 combinedDirection = (direction + avoidance * Stats.AvoidanceWeight).Normalized();

    //         Velocity = direction * speedInThisFrame;
    //         MoveAndSlide();

    //         CurrentState = UnitStates.Moving;
    //         _animationPlayer.Play(RunAnim);
    //         _sprite2D.FlipH = direction.X < 0.0f;

    //         if (this.IsInGroup("PlayerUnits"))
    //         {
    //             CheckFogUpdate();
    //         }

    //     }
    // }

    private void OnReachDestination()
    {
        Stop();

        if (CurrentState == UnitStates.Charging)
        {
            var personalTarget = FindPersonalTargetInEnemySquad();

            if (personalTarget == null) { return; }

            Attack(personalTarget);
        }
        else
        {
            Idle();
        }
    }

    private void OnTargetedByCharge()
    {
        CurrentState = UnitStates.BattleReady;
        _animationPlayer.Play(IdleAnim);
    }

    private void Idle()
    {
        CurrentState = UnitStates.Idle;
        _animationPlayer.Play(IdleAnim);
    }

    private void Stop()
    {
        GlobalPosition = TargetPosition;
        Velocity = Vector2.Zero;
    }

    public void Charge(Vector2 targetPosition)
    {
        TargetPosition = targetPosition;
        CurrentState = UnitStates.Charging;
        _animationPlayer.Play(RunAnim);
        
    }

    private void Attack(Unit targetUnit)
    {
        _currentAttackTarget = targetUnit;
        CurrentState = UnitStates.Attacking;

        _animationPlayer.Play(AttackAnim);
        _sprite2D.FlipH = GlobalPosition.DirectionTo(_currentAttackTarget.GlobalPosition).X < Constants.Zero;
    }

    protected Unit FindPersonalTargetInEnemySquad()
    {
        var enemyUnits = _attackDistanceArea.GetOverlappingBodies()
            .OfType<Unit>()
            .Where(t => t.IsInGroup(EnemyUnitsGroup))
            .OrderBy(t => t.GlobalPosition.DistanceSquaredTo(GlobalPosition));

        foreach (var enemy in enemyUnits)
        {
            if (enemy.UnitAttackers.Count < MaxAttackers)
            {
                enemy.UnitAttackers.Add(this);
                return enemy;
            }
        }
        return null;
    }

    public override void _Draw()
    {
        if (!Debug.IsEnabled) { return; }

        var lineColor = CurrentState == UnitStates.Moving ? Colors.Green : Colors.Blue;

        DrawCircle(Vector2.Zero, Stats.AttackDistance, Colors.Orange with { A = 0.5f });
        DrawLine(Vector2.Zero, ToLocal(TargetPosition), lineColor, 4.0f);
    }

}

