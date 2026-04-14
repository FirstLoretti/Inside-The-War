using Godot;

namespace InsideTheWar.Entities;

public partial class Unit : CharacterBody2D
{
    [ExportGroup("Stats")]
    [Export] protected float _speed = 150.0f;
    [Export] protected int _visionRadius = 1;
  

    [ExportGroup("FormationSettings")]
    [Export] protected int _formationCols = 3;
    [Export] protected int _formationRows= 3;
    [Export] protected int _formationSpacing = 20;
    public int FormationCols => _formationCols;
    public int FormationRows => _formationRows;
    public int FormationSpacing => _formationSpacing;

    [ExportGroup("Technical")]
    [Export] protected float _stoppingDistance = 10.0f;
    [Export] protected float _arrivalDistance = 50.0f;

    private AnimationPlayer _animationPlayer;
    private Sprite2D _sprite2D;

    private Vector2 _targetPosition;
    private int _squadId;
    public int SquadId => _squadId;
    public int MyRow = 0;
    public int MyCol = 0;

    public bool IsMoving => GlobalPosition.DistanceTo(_targetPosition) > _stoppingDistance;

    public override void _Ready()
    {
        base._Ready();
        _targetPosition = GlobalPosition;
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        _sprite2D = GetNode<Sprite2D>("Sprite2D");
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        ProcessMovement();
    }

    public void MoveTo(Vector2 newPosition)
    {
        _targetPosition = newPosition;
    }

    private void ProcessMovement()
    {
        float distance = GlobalPosition.DistanceTo(_targetPosition);

        if (distance < _stoppingDistance)
        {
            if (Velocity != Vector2.Zero)
            {
                GlobalPosition = _targetPosition;
                Velocity = Vector2.Zero;
                _animationPlayer.Play("Idle");
            }
            return;
        }

        Vector2 direction = GlobalPosition.DirectionTo(_targetPosition);
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

