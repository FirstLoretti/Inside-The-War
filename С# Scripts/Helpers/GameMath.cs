using System.Collections.Generic;
using System.Linq;
using Godot;
using InsideTheWar.Entities;

namespace InsideTheWar.Helpers;

public static class GameMath
{
    public static Vector2 CalculateSquadOffset
    (int col, int row, int totalColls, int totalRows, int spacing)
    {
        float offsetX = (col - (totalColls - 1.0f) / 2.0f) * spacing;
        float offsetY = (row - (totalRows - 1.0f) / 2.0f) * spacing;

        return new Vector2(offsetX, offsetY);
    }

    public static IEnumerable<Vector2I> GetCellsInRadius(Vector2I currentCell, int vision)
    {
        for (int x = -vision; x <= vision; x++)
        {
            for (int y = -vision; y <= vision; y++)
            {
                if (x * x + y * y <= (vision + 0.5f) * (vision + 0.5f))
                {
                    yield return currentCell + new Vector2I(x, y);
                }
            }
        }
    }

    public static Vector2 CalculateSquadCenter<T>(IEnumerable<T> units) where T : Unit
    {
        Vector2 squadCenter = Vector2.Zero;
        int count = 0;
        foreach (var unit in units)
        {
            squadCenter += unit.GlobalPosition;
            count++;
        }

        return squadCenter /= count;
    }

    public static Vector2 CalculateAvoidance<T>(Area2D avoidanceArea, T currentUnit) where T : Unit
    {
        var avoidanceVector = Vector2.Zero;
        var neighbors = avoidanceArea.GetOverlappingBodies();

        foreach (var entity in neighbors)
        {
            T neighbor = entity as T;
            if (neighbor == null || neighbor == currentUnit) { continue; }

            var pushDirection = currentUnit.GlobalPosition - neighbor.GlobalPosition;
            var distanceTo = currentUnit.GlobalPosition.DistanceTo(neighbor.GlobalPosition);
            distanceTo = Mathf.Max(distanceTo, 1.0f);

            avoidanceVector += pushDirection.Normalized() / distanceTo;
        }

        return avoidanceVector;
    }

    public static float CalculateSpeedInThisFrame
    (float maxSpeed, float minSpeed, float distanceTo, float arrivalDistance)
    {
        var speedInThisFrame = maxSpeed;

        if (distanceTo < arrivalDistance)
        {
            speedInThisFrame = maxSpeed * (distanceTo / arrivalDistance);
        }

        speedInThisFrame = Mathf.Max(speedInThisFrame, minSpeed);

        return speedInThisFrame;
    }

    public static List<Vector2> GenerateTargetPoints
    (Vector2 mousePos, int count, int unitFormationCols, int unitFormationRows, int unitFormationSpacing)
    {
        List<Vector2> points = new();

        for (int i = 0; i < count; i++)
        {
            int col = i % unitFormationCols;
            int row = i / unitFormationCols;

            var offset = CalculateSquadOffset
            (col, row, unitFormationCols, unitFormationRows, unitFormationSpacing);

            points.Add(mousePos + offset);
        }

        return points;
    }

    public static void AssignUnitsToPointsAlgorithm(IReadOnlyList<PlayerUnit> units, Vector2 mousePosition)
    {
        var points = GenerateTargetPoints
        (mousePosition, units.Count, units[0].FormationCols, units[0].FormationRows, units[0].FormationSpacing);

        var sortedUnits = units
        .OrderBy(u => u.GlobalPosition.X)
        .ThenBy(u => u.GlobalPosition.Y)
        .ToList();
        var sortedPoints = points
        .OrderBy(p => p.X)
        .ThenBy(p => p.Y)
        .ToList();

        for (int i = 0; i < sortedUnits.Count; i++)
        {
            sortedUnits[i].TargetPosition = sortedPoints[i];
        }
    }

    public static Vector2 GetRandomPointInCircle(Vector2 center, float minRadius, float maxRadius)
    {
        RandomNumberGenerator rng = new();
        rng.Randomize();

        var randomRadius = rng.RandfRange(minRadius, maxRadius);
        var randomDirection = rng.RandfRange(0, Mathf.Tau);
        var offset = new Vector2(Mathf.Sin(randomDirection), Mathf.Cos(randomDirection)) * randomRadius;

        return center + offset;
    }
}