using Godot;
using InsideTheWar.Entities.Components;

namespace InsideTheWar.Interfaces;

public interface IDamageable
{
    public HealthComponent HealthComponent { get; }
    public Vector2 GlobalPosition { get; }
}

