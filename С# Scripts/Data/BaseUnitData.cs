using Godot;

namespace InsideTheWar.Data;

[GlobalClass]
public partial class BaseUnitData : Resource
{
    [Export] private float _health = 100.0f;
    [Export] private float _minSpeed = 150.0f;
    [Export] private float _maxSpeed = 125.0f;
    [Export] private float _attackDistance = 60.0f;
    [Export] private float _visionDistance = 200.0f;
    [Export] private int _fogVisionDistance = 1;
    //[Export] public float AvoidanceWeight = 0.2f;

    public float Health => _health;
    public float MinSpeed => _minSpeed;
    public float MaxSpeed => _maxSpeed;
    public float AttackDistance => _attackDistance;
    public float VisionDistance => _visionDistance;
    public int FogVisionDistance => _fogVisionDistance;

}
