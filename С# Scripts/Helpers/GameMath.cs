using System.Collections.Generic;
using Godot;

namespace InsideTheWar.Helpers;

public static class GameMath
{
    public static Vector2 CalculateSquadOffset(int col, int row, int totalColls, int totalRows, int spacing)
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

    public static Vector2 CalculateSquadCenter<T>(IEnumerable<T> units) where T : Node2D
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

    public static Vector2 CalculateAvoidance<T>(Area2D avoidanceArea, T currentUnit) where T : Node2D
    {
        var avoidanceVector = Vector2.Zero;
        var neighbors = avoidanceArea.GetOverlappingBodies();

        foreach (var entity in neighbors)
        {
            T neighbor = entity as T;
            if (neighbor == null || neighbor == currentUnit) { continue; }

            var pushDir = currentUnit.GlobalPosition - neighbor.GlobalPosition;
            var distance = currentUnit.GlobalPosition.DistanceTo(neighbor.GlobalPosition);
            distance = Mathf.Max(distance, 1.0f);

            avoidanceVector += pushDir.Normalized() / distance;
        }

        return avoidanceVector;
    }
/*
    public static float CalculateSpeedInThisFrame<T>(T currentUnit) where T : Node2D
    {
        var speedInThisFrame = currentUnit.Max;

        if (distanceTo < _arrivalDistance)
        {
            speedThisFrame = maxSpeed * (distanceTo / _arrivalDistance);
        }

        speedThisFrame = Mathf.Max(speedThisFrame, minSpeed);
        //GD.Print($"{speedThisFrame}");

        return speedThisFrame;
    }
    */
}