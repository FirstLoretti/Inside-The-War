using Godot;
using InsideTheWar.Interfaces;
using System;
using System.Collections.Generic;

namespace InsideTheWar.Entities.Components;

public partial class HealthComponent : Node
{
    public int Health { get; private set; }
    public int MaxAttackers { get; private set; }
    public List<IDamageable> Attackers { get; private set; } = [];

    public event Action HealthDepleted;

    public void Initialize(int health, int maxAttackers)
    {
        Health = health;
        MaxAttackers = maxAttackers;
    }

    public void TakeDamage(int damage)
    {
        Health -= damage;
        if (Health <= 0)
        {
            HealthDepleted?.Invoke();
        }
    }

    public void ClearAttackersList()
    {
        Attackers.Clear();
    }

    public void AddAttacker(IDamageable attacker)
    {
        Attackers.Add(attacker);
    }
}
