using System.Collections.Generic;
using Godot;

namespace InsideTheWar.Entities;

public partial class PlayerSquad : Node
{
    public List<Unit> Units { get; set; } = new();
}
