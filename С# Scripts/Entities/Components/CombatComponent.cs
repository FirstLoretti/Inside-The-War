using Godot;
using InsideTheWar.Helpers;
using InsideTheWar.Interfaces;
using System.Linq;

namespace InsideTheWar.Entities.Components;

public partial class CombatComponent : Node
{
    public IDamageable PersonalTarget { get; private set; }

    private CollisionObject2D _node;
    private Area2D _attackDistance;
    private StringName _enemyGroup;
    private float _findEnemyTimer;
    private readonly float _timer = 0.1f;

    public void Initialize(
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
            if (enemy.HealthComponent.Attackers.Count < enemy.HealthComponent.MaxAttackers)
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
