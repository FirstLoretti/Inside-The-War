using Godot;

namespace InsideTheWar.Data;

[GlobalClass]
public partial class FormationData : Resource
{
    [Export] public int Cols { get; private set; } = 4;
    [Export] public int Rows { get; private set; } = 4;
    [Export] public int Spacing { get; private set; } = 60;

    public FormationData() { }

    public FormationData(int cols, int rows, int spacing)
    {
        Cols = cols;
        Rows = rows;
        Spacing = spacing;
    }
}
