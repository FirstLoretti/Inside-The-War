using Godot;

namespace InsideTheWar.Entities;

public partial class Unit : CharacterBody2D
{
    [ExportGroup("Stats")]
    [Export] protected float _speed = 150.0f;
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
    [Export] protected float _stoppingDistance { get; set; } = 10.0f;
    [Export] protected float _arrivalDistance { get; set; } = 50.0f;

    private AnimationPlayer _animationPlayer;
    private Sprite2D _sprite2D;

    public Vector2 LastPosition { get; private set; }
    public Vector2 TargetPosition { get; set; }
    public int SquadId;
    public int Row;
    public int Col;

    public bool IsMoving => GlobalPosition.DistanceTo(TargetPosition) > _stoppingDistance;

    public override void _Ready()
    {
        base._Ready();
        LastPosition = GlobalPosition;
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
        float distance = GlobalPosition.DistanceTo(TargetPosition);

        if (distance < _stoppingDistance)
        {
            if (Velocity != Vector2.Zero)
            {
                GlobalPosition = TargetPosition;
                Velocity = Vector2.Zero;
                _animationPlayer.Play("Idle");
            }
            return;
        }

        Vector2 direction = GlobalPosition.DirectionTo(TargetPosition);
        Velocity = direction * _speed;

        if (distance < _arrivalDistance)
        {
            Velocity *= distance / _arrivalDistance;
        }

        MoveAndSlide();
        _animationPlayer.Play("Run");
        _sprite2D.FlipH = direction.X < 0.0f;
    }

}

