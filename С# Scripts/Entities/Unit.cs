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
    [Export] protected UnitData _stats;
    private int _health;
    private int _maxAttackers;
    public UnitData Stats => _stats;
    public float MinSpeed => _stats.MinSpeed;
    public float MaxSpeed => _stats.MaxSpeed;
    public int Health => _health;
    public int MaxAttackers => _maxAttackers;

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
    public List<IDamageable> Attackers => _combat.Attackers;
    public void AddAttacker(IDamageable attacker) => _combat.AddAttacker(attacker);
    public int SquadId { get; set; }
    public int Row { get; set; }
    public int Col { get; set; }
    public ulong Id { get; set; }
    public Vector2 MovementTargetPosition => _movement.TargetPosition;
    public bool IsMoving => GlobalPosition.DistanceTo(MovementTargetPosition) > _stoppingDistance;
    public IDebug Debug { get; set; }
    public StringName EnemyGroup { get; set; }

    public event Action<Unit> Dying;

    protected static readonly StringName RunAnim = "Run";
    protected static readonly StringName IdleAnim = "Idle";
    protected static readonly StringName AttackAnim = "Attack";
    protected const float _stoppingDistance = 5.0f;
    protected const float _stoppingDistanceSqr = _stoppingDistance * _stoppingDistance;
    protected const float _arrivalDistance = 50.0f;
    protected const float _updateFogTriggerDistance = 32.0f;
    protected const float _timer = 0.1f;
    protected const float _checkAllyRayMultiplicator = 3.0f;

    private IDamageable _personalAttackTarget => _combat.PersonalTarget;
    private MovementComponent _movement = new();
    private CombatComponent _combat = new();
    private float _checkAttackQueueTimer;

    public override void _Ready()
    {
        SetAttackAreaRadius();
        AddMovementComponent();
        AddCombatComponent();
        AddToGroup(Constants.Debuggable);
        Id = GetInstanceId();
        CurrentState = UnitStates.WaitingOrder;
        _animationPlayer.Play(IdleAnim);
        _health = Stats.Health;
        _maxAttackers = Stats.MaxAttackers;
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
            if (TickTryStartCombat(deltaFloat))
            {
                return;
            }
            TickAttackQueue(deltaFloat);
            return;
        }

        if (CurrentState == UnitStates.Charging)
        {
            if (TickTryStartCombat(deltaFloat))
            {
                return;
            }
            if (IsFrontAllyOnCombat())
            {
                CurrentState = UnitStates.BattleReady;
                return;
            }
        }

        UpdateMovement(deltaFloat, MovementTargetPosition);
    }

    private bool TickTryStartCombat(float delta)
    {
        if (TickFindAndSetTarget(delta))
        {
            StartCombat(_personalAttackTarget);
            return true;
        }
        return false;
    }

    private void AddMovementComponent()
    {
        AddChild(_movement);
        _movement.Initialization(this, _sprite2D, _stats.MinSpeed, _stats.MaxSpeed, _arrivalDistance);
    }

    private void AddCombatComponent()
    {
        AddChild(_combat);
        _combat.Initialization(this, _attackDistanceArea, EnemyGroup);
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

    private bool IsFrontAllyOnCombat()
    {
        if (GetFrontAlly() is Unit ally)
        {
            if (ally.CurrentState == UnitStates.Attacking || ally.CurrentState == UnitStates.BattleReady)
            {
                return true;
            }
        }
        return false;
    }

    private bool TickFindAndSetTarget(float delta)
    {
        var enemy = _combat.TickFindEnemy(delta);
        if (_combat.TrySetPersonalTarget(enemy))
        {
            return true;
        }
        return false;
    }

    public virtual void MoveTo(Vector2 targetPosition) //! Refactoring
    {
        if (CurrentState != UnitStates.Charging)
        {
            CurrentState = UnitStates.Moving;
        }
        _movement.MoveTo(targetPosition);
        _animationPlayer.Play(RunAnim);
    }

    private void TickAttackQueue(float delta)
    {
        _checkAttackQueueTimer -= delta;
        if (_checkAttackQueueTimer <= Constants.Zero)
        {
            var frontAlly = GetFrontAlly();

            if (frontAlly == null)
            {
                var distanceToTargetSqr = GlobalPosition.DistanceSquaredTo(MovementTargetPosition);
                if (distanceToTargetSqr > _stoppingDistanceSqr)
                {
                    Charge(MovementTargetPosition);
                }
                else
                {
                    var enemy = _combat.FindNearestAvailibleEnemy();
                    if (enemy != null)
                    {
                        StartCombat(enemy);
                    }
                }

            }

            _checkAttackQueueTimer = _timer;
        }
    }

    public IUnit GetFrontAlly()
    {
        return _combat.GetFrontAlly(MovementTargetPosition, _formationSpacing, _checkAllyRayMultiplicator);
    }

    private void OnReachDestination()
    {
        Stop();

        if (CurrentState == UnitStates.Charging)
        {
            var enemy = _combat.FindNearestAvailibleEnemy();
            _combat.TrySetPersonalTarget(enemy);
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
        GlobalPosition = MovementTargetPosition;
        Velocity = Vector2.Zero;
    }

    public void Charge(Vector2 targetPosition)
    {
        _movement.MoveTo(targetPosition);
        CurrentState = UnitStates.Charging;
        _animationPlayer.Play(RunAnim);
    }

    public void BattleReady()
    {
        Velocity = Vector2.Zero;
        CurrentState = UnitStates.BattleReady;
        _animationPlayer.Play(IdleAnim);
    }

    private void Attack(IDamageable target)
    {
        if (_personalAttackTarget == target && CurrentState == UnitStates.Attacking) { return; }

        Velocity = Vector2.Zero;
        CurrentState = UnitStates.Attacking;

        _animationPlayer.Play(AttackAnim);
        var direction = GlobalPosition.DirectionTo(target.GlobalPosition);
        _movement.LookAt(direction);
    }
    #endregion

    private void StartCombat(IDamageable target)
    {
        Attack(target);
        AddAttacker(target);
        TargetCounterattack(target, this);
    }

    private void TargetCounterattack(IDamageable target, IDamageable attacker)
    {
        if (target is Unit enemyUnit)
        {
            enemyUnit._combat.TrySetPersonalTarget(attacker);
            enemyUnit.Attack(attacker);
            enemyUnit.AddAttacker(attacker);
        }
    }

    public void DoDamage() // Animation Event
    {
        var damage = (int)GameMath.GetRandomNumber(Stats.MinDamage, Stats.MaxDamage);
        if (!IsInstanceValid((CollisionObject2D)_personalAttackTarget))
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
        _combat.ClearTarget();

        var nextTarget = _combat.FindNearestAvailibleEnemy();
        if (nextTarget != null)
        {
            StartCombat(nextTarget);
            return;
        }

        var targetPosition = GlobalPosition + _movement.LookDirection * FormationSpacing;
        Charge(targetPosition);
    }

    private void OnDying()
    {
        CurrentState = UnitStates.Dead;
        foreach (var attacker in Attackers)
        {
            if (attacker is Unit unit)
            {
                if (IsInstanceIdValid(unit.Id))
                {
                    unit.OnTargetLost();
                }
            }
        }
        _combat.ClearAttackersList();
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
        var direction = (MovementTargetPosition - GlobalPosition).Normalized();
        var rayDistance = direction * (_formationSpacing * _checkAllyRayMultiplicator);

        DrawCircle(Vector2.Zero, Stats.AttackDistance, Colors.Orange with { A = 0.5f });
        DrawLine(Vector2.Zero, ToLocal(MovementTargetPosition), movementLineColor, 4.0f);
        DrawLine(Vector2.Zero, rayDistance, Colors.Black with { A = 0.3f }, 16.0f);
    }
}

public partial class MovementComponent : Node
{
    public Vector2 TargetPosition { get; private set; }
    public Vector2 LookDirection { get; private set; }

    private CharacterBody2D _body;
    private Sprite2D _sprite;
    private float _arrivalDistance;
    private float _minSpeed;
    private float _maxSpeed;

    public void Initialization(
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

public partial class CombatComponent : Node
{
    public IDamageable PersonalTarget { get; private set; }
    public List<IDamageable> Attackers { get; private set; } = [];

    private CollisionObject2D _node;
    private Area2D _attackDistance;
    private StringName _enemyGroup;
    private float _findEnemyTimer;
    private readonly float _timer = 0.1f;

    public void Initialization(
        CollisionObject2D collisionObject2D,
        Area2D attackDistance,
        StringName enemyGroup
    )
    {
        _node = collisionObject2D;
        _attackDistance = attackDistance;
        _enemyGroup = enemyGroup;
    }

    public IDamageable FindNearestAvailibleEnemy()
    {
        var enemies = _attackDistance.GetOverlappingBodies()
        .OfType<IDamageable>()
        .Where(e => e is Node2D node && node.IsInGroup(_enemyGroup))
        .OrderBy(e => e.GlobalPosition.DistanceSquaredTo(_node.GlobalPosition));

        foreach (var enemy in enemies)
        {
            if (enemy.Attackers.Count < enemy.MaxAttackers)
            {
                return enemy;
            }
        }

        return null;
    }

    public bool TrySetPersonalTarget(IDamageable enemy)
    {
        if (enemy != null)
        {
            PersonalTarget = enemy;
            return true;
        }
        else
        {
            return false;
        }
    }

    public void ClearAttackersList()
    {
        Attackers.Clear();
    }

    public void AddAttacker(IDamageable attacker)
    {
        Attackers.Add(attacker);
    }

    public IDamageable TickFindEnemy(float delta)
    {
        _findEnemyTimer -= delta;
        if (_findEnemyTimer <= Constants.Zero)
        {
            _findEnemyTimer = _timer;
            var enemy = FindNearestAvailibleEnemy();
            return enemy;
        }
        return null;
    }

    public void ClearTarget()
    {
        PersonalTarget = null;
    }

    public IUnit GetFrontAlly(Vector2 movementTargetPosition, float formationSpacing, float rayMultiplicator)
    {
        var direction = _node.GlobalPosition.DirectionTo(movementTargetPosition);
        var rayDistance =
            _node.GlobalPosition +
            direction *
            formationSpacing *
            rayMultiplicator;
        var spaceState = _node.GetWorld2D().DirectSpaceState;

        var ray = PhysicsRayQueryParameters2D.Create(_node.GlobalPosition, rayDistance);
        ray.Exclude = [_node.GetRid()];
        ray.CollisionMask = _node.CollisionLayer;

        var result = spaceState.IntersectRay(ray);
        if (result.Count > Constants.Zero)
        {
            var collider = result["collider"].As<Node2D>();
            if (collider is IUnit ally)
            {
                return ally;
            }
        }

        return null;
    }
}
