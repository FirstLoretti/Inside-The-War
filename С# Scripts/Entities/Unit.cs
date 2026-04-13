using Godot;

namespace InsideTheWar.Entities;

public partial class Unit : CharacterBody2D
{
    [Export] protected float Speed { get; private set; } = 150.0f;
    [Export] protected float ArrivalDistance { get; private set; } = 25.0f;

    [ExportGroup("FormationSettings")]
    [Export] protected int FormationCols { get; private set; } = 3;
    [Export] protected int FormationRows { get; private set; } = 3;
    [Export] protected int FormationSpacing { get; private set; } = 20;

    private AnimationPlayer _animationPlayer;
    private Sprite2D _sprite2D;

    protected Vector2 targetPosition;

    public bool IsMoving => GlobalPosition.DistanceTo(targetPosition) > 5.0f;

    public override void _Ready()
    {
        base._Ready();
        targetPosition = GlobalPosition;
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        _sprite2D = GetNode<Sprite2D>("Sprite2D");
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        MoveToTarget();
    }

    private void MoveToTarget()
    {
        float distance = GlobalPosition.DistanceTo(targetPosition);

        if (distance < 5.0f)
        {
            if (Velocity != Vector2.Zero)
            {
                GlobalPosition = targetPosition;
                Velocity = Vector2.Zero;
                _animationPlayer.Play("Idle");
            }
            return;
        }

        Vector2 direction = GlobalPosition.DirectionTo(targetPosition);
        Velocity = direction * Speed;

        if (distance < ArrivalDistance)
        {
            Velocity *= distance / ArrivalDistance;
        }

        MoveAndSlide();
        _animationPlayer.Play("Run");
        _sprite2D.FlipH = direction.X < 0.0f;

    }

}

