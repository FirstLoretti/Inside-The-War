using Godot;
using InsideTheWar.Helpers;
using InsideTheWar.Singletons;
using InsideTheWar.Data;

namespace InsideTheWar.Entities;

public partial class Unit : CharacterBody2D
{
    [ExportGroup("Status")]
    [Export] public UnitStates CurrentState { get; protected set;} = UnitStates.Idle;

    [ExportGroup("Stats")]
    [Export] public BaseUnitData Stats { get; private set; }
    [Export] protected float _maxSpeed = 150.0f;
    [Export] protected float _minSpeed = 75.0f;
    [Export] protected int _visionRadius = 1;
    public int VisionRadius => _visionRadius;

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
    [Export] private float _updateSignalsTreshold = 32.0f;
    [Export] private Area2D _avoidanceArea;

    private AnimationPlayer _animationPlayer;
    private Sprite2D _sprite2D;

    public Vector2 TargetPosition { get; set; }
    public Vector2 LastSignaledPos { get; set; }
    public int SquadId { get; set; }
    public int Row { get; set; }
    public int Col { get; set; }

    public bool IsMoving => GlobalPosition.DistanceTo(TargetPosition) > _stoppingDistance;

    public override void _Ready()
    {
        base._Ready();

        TargetPosition = GlobalPosition;
        LastSignaledPos = GlobalPosition;

        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        _sprite2D = GetNode<Sprite2D>("Sprite2D");
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        ProcessMovement();
    }

    private void ProcessMovement()
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
            _animationPlayer.Play("Idle");
        }
        else
        {
            var direction = GlobalPosition.DirectionTo(TargetPosition);
            //var avoidance = GameMath.CalculateAvoidance(_avoidanceArea, this);
            var speedInThisFrame = GameMath.CalculateSpeedInThisFrame
            (_maxSpeed, _minSpeed, distanceTo, _arrivalDistance);
            //Vector2 combinedDirection = (direction + avoidance * Stats.AvoidanceWeight).Normalized();

            Velocity = direction * speedInThisFrame;
            MoveAndSlide();

            _animationPlayer.Play("Run");
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

