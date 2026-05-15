using Godot;
using InsideTheWar.Helpers;
using InsideTheWar.Data;
using System.Collections.Generic;
using InsideTheWar.Interfaces;
using System.Linq;
using System;

namespace InsideTheWar.Entities;

public partial class Unit : CharacterBody2D, IUnit, IDamageable
{
    [ExportGroup("Stats")]
    [Export] protected BaseUnitData _stats;
    public BaseUnitData Stats => _stats;
    private int _health;

    [ExportGroup("FormationSettings")]
    [Export] protected int _formationCols = 3;
    [Export] protected int _formationRows = 3;
    [Export] protected int _formationSpacing = 20;
    public int FormationCols => _formationCols;
    public int FormationRows => _formationRows;
    public int FormationSpacing => _formationSpacing;

    [ExportGroup("Dependencies")]
    [Export] protected Area2D _attackDistanceArea;
    [Export] protected AnimationPlayer _animationPlayer;
    [Export] protected Sprite2D _sprite2D;

    #region // IfAvoidanceOn
    //[Export] protected Area2D _avoidanceArea;
    #endregion

    public virtual UnitStates CurrentState { get; protected set; }
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

    public event Action<Unit> Dying;

    protected static readonly StringName RunAnim = "Run";
    protected static readonly StringName IdleAnim = "Idle";
    protected static readonly StringName AttackAnim = "Attack";
    protected const float _stoppingDistance = 5.0f;
    protected const float _stoppingDistanceSqr = _stoppingDistance * _stoppingDistance;
    protected const float _arrivalDistance = 50.0f;
    protected const float _updateFogTriggerDistance = 32.0f;
    protected const float _timer = 0.1f;
    private const float _checkAllyRayMultiplicator = 3.0f;

    private Unit _personalAttackTarget;
    private float _findPersonalTargetTimer;
    private float _getFrontAllyTimer;
    private float _checkAttackQueueTimer;
    private Vector2 _lookDirection;

    public override void _Ready()
    {
        TargetPosition = GlobalPosition;
        Id = GetInstanceId();
        CurrentState = UnitStates.WaitingOrder;
        AddToGroup(Constants.Debuggable);
        _animationPlayer.Play(IdleAnim);
        _health = Stats.Health;
        SetAttackAreaRadius();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Debug.IsEnabled)
        {
            QueueRedraw();
        }

        var deltaFloat = (float)delta;

        if (CurrentState != UnitStates.Moving &&
            CurrentState != UnitStates.Charging &&
            CurrentState != UnitStates.BattleReady)
        {
            return;
        }

        if (CurrentState == UnitStates.BattleReady)
        {
            TickCheckAttackQueueTimer(deltaFloat);
            return;
        }

        UpdateMovement(deltaFloat, TargetPosition);
    }
    protected virtual void UpdateMovement(float delta, Vector2 targetPosition)
    {
        if (CurrentState == UnitStates.Charging)
        {
            CheckForAttack(delta);

            if (CurrentState == UnitStates.Attacking) { return; }

            var frontAlly = GetFrontAlly();
            if (frontAlly?.CurrentState == UnitStates.Attacking || frontAlly?.CurrentState == UnitStates.BattleReady)
            {
                BattleReady();
                return;
            }
        }

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

    public virtual void MoveTo(Vector2 targetPosition) //! Refactoring
    {
        if (CurrentState != UnitStates.Charging)
        {
            CurrentState = UnitStates.Moving;
        }
        TargetPosition = targetPosition;

        var direction = GlobalPosition.DirectionTo(targetPosition);
        if (direction != Vector2.Zero)
        {
            _lookDirection = direction;
        }
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

    private void TickCheckAttackQueueTimer(float delta)
    {
        _checkAttackQueueTimer -= delta;
        if (_checkAttackQueueTimer <= Constants.Zero)
        {
            var frontAlly = GetFrontAlly();

            if (frontAlly == null)
            {
                var distanceToTargetSqr = GlobalPosition.DistanceSquaredTo(TargetPosition);
                if (distanceToTargetSqr > _stoppingDistanceSqr)
                {
                    Charge(TargetPosition);
                }
                else
                {
                    var enemy = FindPersonalTargetInEnemySquad();
                    if (enemy != null)
                    {
                        StartCombat(enemy);
                    }
                }

            }

            _checkAttackQueueTimer = _timer;
        }
    }

    public Unit GetFrontAlly()
    {
        var direction = (TargetPosition - GlobalPosition).Normalized();
        var rayDistance = GlobalPosition + direction * (_formationSpacing * _checkAllyRayMultiplicator);
        var spaceState = GetWorld2D().DirectSpaceState;
        var ray = PhysicsRayQueryParameters2D.Create(GlobalPosition, rayDistance);
        ray.Exclude = new Godot.Collections.Array<Rid> { GetRid() };
        ray.CollisionMask = 2;
        var result = spaceState.IntersectRay(ray);
        if (result.Count > 0)
        {
            var resultCollider = result["collider"].As<Node2D>();
            if (resultCollider is Unit ally)
            {
                return ally;
            }
        }
        return null;
    }

    private void OnReachDestination()
    {
        Stop();

        if (CurrentState == UnitStates.Charging)
        {
            _personalAttackTarget = FindPersonalTargetInEnemySquad();

            if (_personalAttackTarget != null)
            {
                StartCombat(_personalAttackTarget);
            }
            else
            {
                BattleReady();
            }
        }
        else
        {
            Idle();
        }
    }

    private void SetAttackAreaRadius()
    {
        var collisionShape = _attackDistanceArea.GetChild<CollisionShape2D>(0);
        var circleShape = (CircleShape2D)collisionShape.Shape;
        circleShape.Radius = Stats.AttackDistance;
    }

    #region States
    public void Idle()
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

    public void BattleReady()
    {
        Velocity = Vector2.Zero;
        CurrentState = UnitStates.BattleReady;
        _animationPlayer.Play(IdleAnim);
    }

    private void Attack(Unit targetUnit)
    {
        if (_personalAttackTarget == targetUnit && CurrentState == UnitStates.Attacking) { return; }

        Velocity = Vector2.Zero;
        _personalAttackTarget = targetUnit;
        CurrentState = UnitStates.Attacking;

        _animationPlayer.Play(AttackAnim);
        _sprite2D.FlipH = GlobalPosition.DirectionTo(_personalAttackTarget.GlobalPosition).X < Constants.Zero;
    }
    #endregion

    private void StartCombat(Unit targetUnit)
    {
        Attack(targetUnit);
        targetUnit.Attack(this);
    }

    private void CheckForAttack(float delta)
    {
        TickFindPersonalTargetTimer(delta);
        if (_personalAttackTarget != null)
        {
            StartCombat(_personalAttackTarget);
        }
    }

    private Unit FindPersonalTargetInEnemySquad()
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

    private void TickFindPersonalTargetTimer(float delta)
    {
        _findPersonalTargetTimer -= delta;
        if (_findPersonalTargetTimer <= Constants.Zero)
        {
            _personalAttackTarget = FindPersonalTargetInEnemySquad();
            _findPersonalTargetTimer = _timer;
        }
    }

    public void DoDamage() // Animation Event
    {
        var damage = (int)GameMath.GetRandomNumber(Stats.MinDamage, Stats.MaxDamage);
        if (!IsInstanceValid(_personalAttackTarget))
        {
            OnTargetLost();
            return;
        }

        _personalAttackTarget.TakeDamage(damage);
    }

    public void TakeDamage(int damage)
    {
        _health -= damage;
        if (_health <= 0)
        {
            OnDying();
            Die();
        }
    }

    public void OnTargetLost()
    {
        _personalAttackTarget = null;

        var nextTarget = FindPersonalTargetInEnemySquad();
        if (nextTarget != null)
        {
            StartCombat(nextTarget);
            return;
        }

        var targetPosition = GlobalPosition + _lookDirection * FormationSpacing;
        Charge(targetPosition);
    }

    private void OnDying()
    {
        CurrentState = UnitStates.Dead;
        foreach (var unit in UnitAttackers)
        {
            if (IsInstanceIdValid(unit.Id))
            {
                unit.OnTargetLost();
            }
        }
        UnitAttackers.Clear();
        Dying?.Invoke(this);
    }

    private void Die()
    {
        QueueFree();
    }

    public override void _Draw()
    {
        if (!Debug.IsEnabled) { return; }

        var movementLineColor = CurrentState == UnitStates.Moving ? Colors.Green : Colors.Blue;
        var direction = (TargetPosition - GlobalPosition).Normalized();
        var rayDistance = direction * (_formationSpacing * _checkAllyRayMultiplicator);

        DrawCircle(Vector2.Zero, Stats.AttackDistance, Colors.Orange with { A = 0.5f });
        DrawLine(Vector2.Zero, ToLocal(TargetPosition), movementLineColor, 4.0f);
        DrawLine(Vector2.Zero, rayDistance, Colors.Black with { A = 0.3f }, 16.0f);
    }
}

