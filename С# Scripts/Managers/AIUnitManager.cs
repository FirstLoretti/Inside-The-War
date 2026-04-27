using Godot;
using InsideTheWar.Entities;
using System.Collections.Generic;

public partial class AIUnitManager : Node
{
    private Dictionary<int, List<AIUnit>> Units = new();
}
