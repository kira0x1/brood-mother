using System;

namespace Kira;

public interface IHealthComponent
{
    // public LifeState LifeState { get; }
    public float MaxHealth { get; }
    public float Health { get; }
    public void TakeDamage(float damage, Vector3 position, Vector3 force, Guid attackerId, DamageType damageType = DamageType.BULLET, bool isHeadshot = false);
}

public enum DamageType
{
    BULLET,
    FIRE,
    BLUNT,
    SHARP
}