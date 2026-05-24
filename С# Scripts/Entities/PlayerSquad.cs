using Godot;

namespace InsideTheWar.Entities;

public partial class PlayerSquad : Squad
{
    public void OnChargeInput(Vector2 mousePosition, StringName enemyGroup)
    {
        var spaceState = GetWorld2D().DirectSpaceState;
        var query = new PhysicsPointQueryParameters2D();
        query.Position = mousePosition;
        query.CollideWithBodies = true;
        var results = spaceState.IntersectPoint(query, maxResults: 1);

        if (results.Count > 0)
        {
            var node = results[0]["collider"].As<Node2D>();
            if (node.IsInGroup(enemyGroup) && node is Unit enemyUnit)
            {
                _currentTargetSquadId = enemyUnit.SquadId;
                TargetUpdateAndCharge();
            }
        }
    }
}
