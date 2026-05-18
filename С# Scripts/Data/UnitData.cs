using Godot;

namespace InsideTheWar.Data;

[GlobalClass]
public partial class UnitData : Resource
{
    [Export] private int _health = 100;
    [Export] private float _minSpeed = 150.0f;
    [Export] private float _maxSpeed = 125.0f;
    [Export] private float _attackDistance = 60.0f;
    [Export] private int _fogVisionDistance = 1;
    [Export] private int _minDamage = 20;
    [Export] private int _maxDamage = 30;
    [Export] private int _maxAttackers = 1;

    #region IfAvoidanceOn
    //[Export] public float AvoidanceWeight = 0.2f;
    #endregion

    public int Health => _health;
    public float MinSpeed => _minSpeed;
    public float MaxSpeed => _maxSpeed;
    public float AttackDistance => _attackDistance;
    public int FogVisionDistance => _fogVisionDistance;
    public int MinDamage => _minDamage;
    public int MaxDamage => _maxDamage;
    public int MaxAttackers => _maxAttackers;
}
