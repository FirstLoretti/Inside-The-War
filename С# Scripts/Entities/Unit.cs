using Godot;
using InsideTheWar.Helpers;
using InsideTheWar.Singletons;
using InsideTheWar.Data;

namespace InsideTheWar.Entities;

public partial class Unit : CharacterBody2D, IUnit
{
    [ExportGroup("Status")]
    [Export] public UnitStates CurrentState { get; set; } = UnitStates.Idle;

    [ExportGroup("Stats")]
    [Export] public BaseUnitData Stats { get; private set; }

    public int VisionRadius => Stats.Vision;

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

    public Vector2 TargetPosition { get; set; }
    public Vector2 LastSignaledPos { get; set; }
    public int SquadId { get; set; }
    public int Row { get; set; }
    public int Col { get; set; }
    public ulong Id {get; set;}
    public bool isLeader = false;

    public bool IsMoving => GlobalPosition.DistanceTo(TargetPosition) > _stoppingDistance;

    protected static readonly StringName RunAnim = "Run";
    protected static readonly StringName IdleAnim = "Idle";

    public override void _Ready()
    {
        base._Ready();

        TargetPosition = GlobalPosition;
        LastSignaledPos = GlobalPosition;
        Id = GetInstanceId();

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
                GetInstanceId(), LastSignaledPos, GlobalPosition, VisionRadius);

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
            GetInstanceId(), LastSignaledPos, GlobalPosition, VisionRadius);

            LastSignaledPos = GlobalPosition;
        }
    }

}

