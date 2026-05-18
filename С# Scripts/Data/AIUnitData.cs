using Godot;

namespace InsideTheWar.Data;

public partial class AIUnitData : UnitData
{
    [Export] private float _minIdleTime = 2.0f;
    [Export] private float _maxIdleTime = 6.0f;
    [Export] private float _visionDistance = 200.0f;
    public float VisionDistance => _visionDistance;
    public float MinIdleTime => _minIdleTime;
    public float MaxIdleTime => _maxIdleTime;
}
