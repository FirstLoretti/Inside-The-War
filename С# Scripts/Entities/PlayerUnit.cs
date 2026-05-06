using Godot;

namespace InsideTheWar.Entities;

public partial class PlayerUnit : Unit
{
    public bool IsSelected = false;

    public void MoveTo(Vector2 target)
    {
        TargetPosition = target;
    }
}
