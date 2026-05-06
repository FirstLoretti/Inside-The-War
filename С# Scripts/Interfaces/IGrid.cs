using Godot;

namespace InsideTheWar.Interfaces;

public interface IGrid
{
    bool IsAreaFree(Vector2I startCell, int cols, int rows);
    int CellSize { get; }
}
