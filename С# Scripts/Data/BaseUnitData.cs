using System.Dynamic;
using Godot;

namespace InsideTheWar.Data;

[GlobalClass]
public partial class BaseUnitData : Resource
{
    [Export] public float Health {get; private set;} = 100.0f; 
    [Export] public float Speed {get; private set;} = 150.0f;
    [Export] public int Vision = 1;
    [Export] public float AvoidanceWeight = 0.2f;
}
