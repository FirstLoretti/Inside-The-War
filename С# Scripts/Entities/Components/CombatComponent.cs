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
    private readonly Vector2[] _rayDirections = new Vector2[3];

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
        .OrderBy(e => e.GlobalPosition.DistanceSquaredTo(_node.GlobalPosition))
        .ToList();

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

    public void ClearTarget()
    {
        PersonalTarget = null;
    }

    public IUnit GetFrontAlly(Vector2 attackDirection, float formationSpacing, float rayMultiplicator)
    {
        var rayLength = formationSpacing * rayMultiplicator;
        var spaceState = _node.GetWorld2D().DirectSpaceState;
        FillRayDirections(attackDirection);

        foreach (var rayDirection in _rayDirections)
        {
            var rayDistance = _node.GlobalPosition + rayDirection * rayLength;
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
        }

        return null;
    }

    private Vector2[] FillRayDirections(Vector2 centerDirection)
    {
        _rayDirections[0] = centerDirection;
        _rayDirections[1] = centerDirection.Rotated(Mathf.DegToRad(-15.0f));
        _rayDirections[2] = centerDirection.Rotated(Mathf.DegToRad(15.0f));

        return _rayDirections;
    }
}
