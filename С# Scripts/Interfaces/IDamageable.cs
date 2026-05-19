using System.Collections.Generic;
using Godot;

namespace InsideTheWar.Interfaces;

public interface IDamageable
{
    public void TakeDamage(int damage);
    public void AddAttacker(IDamageable attacker);
    public List<IDamageable> Attackers { get; }
    public Vector2 GlobalPosition { get; }
    public int Health { get; }
    public int MaxAttackers {get;}
}

