using Godot;
using InsideTheWar.Helpers;
using InsideTheWar.Singletons;
using InsideTheWar.Data;
using System.Collections.Generic;

namespace InsideTheWar.Entities;

public partial class Unit : CharacterBody2D, IUnit
{
    [ExportGroup("Stats")]
    [Export] protected BaseUnitData _stats;
    [Export] protected Area2D _visionArea; //! Оптимизировать
    [Export] protected Area2D _attackDistanceArea;
    public float AttackDistanceRadius { get; private set; }
    public BaseUnitData Stats => _stats;

    [ExportGroup("FormationSettings")]
    [Export] protected int _formationCols = 3;
    [Export] protected int _formationRows = 3;
    [Export] protected int _formationSpacing = 20;
    public int FormationCols => _formationCols;
    public int FormationRows => _formationRows;
    public int FormationSpacing => _formationSpacing;

    [ExportGroup("Technical")]
    [Export] protected float _stoppingDistance = 5.0f;
    [Export] protected float _arrivalDistance = 50.0f;
    [Export] protected float _updateSignalsTreshold = 32.0f;

    [ExportGroup("Dependencies")]
    [Export] protected Area2D _avoidanceArea;
    [Export] protected AnimationPlayer _animationPlayer;
    [Export] protected Sprite2D _sprite2D;

    public UnitStates CurrentState { get; protected set; }
    public List<Unit> UnitsAttackingMe = new();
    public Vector2 TargetPosition { get; set; }
    public Vector2 LastSignaledPos { get; set; }
    public int SquadId { get; set; }
    public int Row { get; set; }
    public int Col { get; set; }
    public ulong Id { get; set; }
    public const int MaxUnitsAttackers = 1;

    public bool IsMoving => GlobalPosition.DistanceTo(TargetPosition) > _stoppingDistance;

    protected static readonly StringName RunAnim = "Run";
    protected static readonly StringName IdleAnim = "Idle";
    protected static readonly StringName AttackAnim = "Attack";

    public override void _Ready()
    {
        base._Ready();

        TargetPosition = GlobalPosition;
        LastSignaledPos = GlobalPosition;
        Id = GetInstanceId();
        AttackDistanceRadius = ((CircleShape2D)((_attackDistanceArea.GetChild<CollisionShape2D>(0)).Shape)).Radius;
        CurrentState = UnitStates.WaitingOrder;
        AddToGroup("Debuggable"); //! Сделать константу

        _animationPlayer.Play(IdleAnim);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        ProcessMovement((float)delta);
    }

    protected virtual void ProcessMovement(float delta)
    {

        float distanceTo = GlobalPosition.DistanceTo(TargetPosition);

        if (distanceTo <= _stoppingDistance)
        {
            GlobalPosition = TargetPosition;

            if (GlobalPosition != LastSignaledPos)
            {
                GlobalSignals.Instance.EmitSignal(GlobalSignals.SignalName.EntityMoved,
                GetInstanceId(), LastSignaledPos, GlobalPosition, Stats.VisionDistance);

                LastSignaledPos = GlobalPosition;
            }

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

            CurrentState = UnitStates.Moving;
            _animationPlayer.Play(RunAnim);
            _sprite2D.FlipH = direction.X < 0.0f;

            CheckFogUpdate();
        }
    }

    private void CheckFogUpdate()
    {
        if (GlobalPosition.DistanceTo(LastSignaledPos) > _updateSignalsTreshold)
        {
            GlobalSignals.Instance.EmitSignal(GlobalSignals.SignalName.EntityMoved,
            GetInstanceId(), LastSignaledPos, GlobalPosition, Stats.VisionDistance);

            LastSignaledPos = GlobalPosition;
        }
    }

    public override void _Draw()
    {
        if (!GlobalDebugManager.IsEnabled) { return; }

        base._Draw();

        var lineColor = CurrentState == UnitStates.Moving ? Colors.Green : Colors.Blue;

        DrawCircle(Vector2.Zero, Stats.AttackDistance, Colors.Orange with { A = 0.25f });
        DrawLine(Vector2.Zero, ToLocal(TargetPosition), lineColor, 4.0f);
        DrawCircle(Vector2.Zero, Stats.VisionDistance, Colors.Yellow with { A = 0.2f });
    }


}

