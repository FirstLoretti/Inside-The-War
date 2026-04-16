using System.Collections.Generic;
using Godot;

namespace InsideTheWar.Helpers;

public static class GameMath
{
    public static Vector2 CalculateOffset(int col, int row, int totalColls, int totalRows, int spacing)
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
}