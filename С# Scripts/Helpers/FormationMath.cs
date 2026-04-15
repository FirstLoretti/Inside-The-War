using Godot;
using InsideTheWar.Entities;

namespace InsideTheWar.Helpers;

public static class FormationMath
{
    public static Vector2 CalculateOffset(int col, int row, int totalColls, int totalRows, int spacing)
    {
        float offset_x = (col - (totalColls - 1.0f) / 2.0f) * spacing;
        float offset_y = (row - (totalRows - 1.0f) / 2.0f) * spacing;

        return new Vector2(offset_x, offset_y);
    }
}
