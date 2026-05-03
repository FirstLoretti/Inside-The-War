using System.Collections.Generic;
using System.Linq;
using Godot;
using InsideTheWar.Entities;

namespace InsideTheWar.Helpers;

public static class GameMath
{
    private static readonly RandomNumberGenerator _rng = new();

    static GameMath()
    {
        _rng.Randomize();
    }

    public static float GetRandomNumber(float number1, float number2) => _rng.RandfRange(number1, number2);

    public static Vector2 CalculateSquadOffset(int col, int row, int formationCols, int formationRows, int spacing)
    {
        float offsetX = (col - (formationCols - 1.0f) / 2.0f) * spacing;
        float offsetY = (row - (formationRows - 1.0f) / 2.0f) * spacing;

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

    public static Vector2 CalculateSquadCenter(IEnumerable<Node2D> units)
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

    public static float CalculateSpeedInThisFrame(float maxSpeed, float minSpeed, float distanceTo, float arrivalDistance)
    {
        var speedInThisFrame = maxSpeed;

        if (distanceTo < arrivalDistance)
        {
            speedInThisFrame = maxSpeed * (distanceTo / arrivalDistance);
        }

        speedInThisFrame = Mathf.Max(speedInThisFrame, minSpeed);

        return speedInThisFrame;
    }
    
    //! Объединение с CalculateSquadOffset?
    public static List<Vector2> GenerateTargetPoints(Vector2 mousePos, int count, int FormationCols, int FormationRows, int FormationSpacing)
    {
        List<Vector2> points = new();

        for (int i = 0; i < count; i++)
        {
            int col = i % FormationCols;
            int row = i / FormationCols;

            var offset = CalculateSquadOffset
            (col, row, FormationCols, FormationRows, FormationSpacing);

            points.Add(mousePos + offset);
        }

        return points;
    }

    public static Dictionary<IUnit, Vector2> AssignUnitsToPointsAlgorithm(IReadOnlyList<IUnit> units, Vector2 squadTargetPosition)
    {
        Dictionary<IUnit, Vector2> assignments = new();

        var points = GenerateTargetPoints(squadTargetPosition, units.Count, units[0].FormationCols, units[0].FormationRows, units[0].FormationSpacing);

        var squadCenter = CalculateSquadCenter(units.Cast<Node2D>());
        var direction = (squadTargetPosition - squadCenter).Normalized();

        Vector2 side = new(-direction.Y, direction.X);

        var sortedUnits = units
            .OrderBy(u => u.GlobalPosition.Dot(direction))
            .ThenBy(u => u.GlobalPosition.Dot(side))
            .ToList();
        var sortedPoints = points
            .OrderBy(p => p.Dot(direction))
            .ThenBy(p => p.Dot(side))
            .ToList();

        for (int i = 0; i < sortedUnits.Count; i++)
        {
            assignments.Add(sortedUnits[i], sortedPoints[i]);
        }

        return assignments;
    }

    public static Vector2 GetRandomPointInCircle(Vector2 center, float minRadius, float maxRadius)
    {
        var randomRadius = GetRandomNumber(minRadius, maxRadius);
        var randomDirection = GetRandomNumber(0, Mathf.Tau);
        var offset = new Vector2(Mathf.Sin(randomDirection), Mathf.Cos(randomDirection)) * randomRadius;

        return center + offset;
    }
}