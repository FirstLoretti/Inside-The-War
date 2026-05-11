namespace InsideTheWar.Interfaces;

public interface IDamageable
{
    public int Health { get; set; }
    public void TakeDamage(int damage);
}

