using Godot;

namespace InsideTheWar.Data;

[GlobalClass]
public partial class FormationData : Resource
{
    [Export] public int Cols { get; protected set; } = 4;
    [Export] public int Rows { get; protected set; } = 4;
    [Export] public int Spacing { get; protected set; } = 60;
}
