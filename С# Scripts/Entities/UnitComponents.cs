using Godot;
using InsideTheWar.Helpers;
using InsideTheWar.Data;
using InsideTheWar.Interfaces;
using System;
using InsideTheWar.Entities.Components;

namespace InsideTheWar.Entities;

public partial class Unit : CharacterBody2D
{
    [Export] public UnitData Data { get; private set; }
    [Export] public FormationData FormationData { get; private set; }

    [ExportGroup("Dependencies")]
    [Export] protected Area2D _attackDistance;
    [Export] protected AnimationPlayer _animationPlayer;
    [Export] protected Sprite2D _sprite2D;

    public virtual UnitStates CurrentState { get; protected set; }
    public int SquadId { get; set; }
    public int Row { get; set; }
    public int Col { get; set; }
    public ulong Id { get; set; }
    public Vector2 MovementTargetPosition => _movement.TargetPosition;
    public bool IsMoving => GlobalPosition.DistanceTo(MovementTargetPosition) > _stoppingDistance;
    public IDebug Debug { get; set; }
    public StringName EnemyGroup { get; set; }
    public const float CheckAllyRayMultiplicator = 3.0f;

    public event Action<Unit> Die;

    protected static readonly StringName _runAnimation = "Run";
    protected static readonly StringName _idleAnimation = "Idle";
    protected static readonly StringName _attackAnimation = "Attack";
    protected const float _stoppingDistance = 5.0f;
    protected const float _stoppingDistanceSqr = _stoppingDistance * _stoppingDistance;
    protected const float _arrivalDistance = 50.0f;
    protected const float _updateFogTriggerDistance = 32.0f;
    protected const float _timer = 0.1f;

    private IDamageable _personalAttackTarget => _combat.PersonalTarget;
    private MovementComponent _movement = new();
    private CombatComponent _combat = new();
    private UnitDebuger _debuger = new();
    public HealthComponent HealthComponent { get; } = new();
    private float _checkAttackQueueTimer;

    public override void _Ready()
    {
        InitializeComponents();
        AddToGroup(Constants.Debuggable);
        Id = GetInstanceId();
        SetState(UnitStates.WaitingOrder);
    }

    private void InitializeComponents()
    {
        SetAttackDistance();
        AddMovementComponent();
        AddCombatComponent();
        AddDebuger();
        AddHealthComponent();
    }

    private void AddDebuger()
    {
        AddChild(_debuger);
        _debuger.Initialize(this);
    }

    private void AddMovementComponent()
    {
        AddChild(_movement);
        _movement.Initialize(this, _sprite2D, Data.MinSpeed, Data.MaxSpeed, _arrivalDistance);
    }

    private void AddCombatComponent()
    {
        AddChild(_combat);
        _combat.Initialize(this, _attackDistance, EnemyGroup);
    }

    private void SetAttackDistance()
    {
        var collisionShape = _attackDistance.GetChild<CollisionShape2D>(0);
        var circleShape = (CircleShape2D)collisionShape.Shape;
        circleShape.Radius = Data.AttackDistance;
    }

    private void AddHealthComponent()
    {
        AddChild(HealthComponent);
        HealthComponent.Initialize(Data.Health, Data.MaxAttackers);
        HealthComponent.HealthDepleted += Dying;
    }
}
