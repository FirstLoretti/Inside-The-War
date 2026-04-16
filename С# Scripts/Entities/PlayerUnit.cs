using Godot;

namespace InsideTheWar.Entities;

public partial class PlayerUnit : Unit
{
    public bool IsSelected = false;

    public override void _Ready()
    {
        base._Ready();
    }

}
