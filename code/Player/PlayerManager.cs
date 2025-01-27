using System;

namespace Kira;

[Group("Kira/Player")]
[Title("Player Manager")]
[Icon("person")]
public sealed class PlayerManager : Component, IHealthComponent
{
    [Property] public bool IsInvincible { get; private set; } = false;

    [Property] public float MaxHealth { get; private set; } = 100;
    [Property] public float Health { get; private set; } = 100;

    public int Gold { get; set; }
    public int CurXp { get; set; }


    [Property] public int HeadshotScoreIncrease = 10;
    public int Score { get; set; } = 0;
    public int TotalKills { get; set; } = 0;

    public AnimationController Animator { get; set; }
    public WeaponManager WeaponManager;
    public Inventory Inventory { get; set; }
    public static PlayerManager Instance { get; set; }

    public enum PlayerStates
    {
        ALIVE,
        DEAD
    }

    public PlayerStates PlayerState { get; set; } = PlayerStates.ALIVE;

    protected override void OnAwake()
    {
        base.OnAwake();
        Instance = this;
        Animator = Components.Get<AnimationController>();
        Inventory = Components.Get<Inventory>();
        WeaponManager = GameObject.Components.Get<WeaponManager>();
    }

    public bool TryGiveItem(GameObject WeaponPrefab, bool ignorePickUpCD = false)
    {
        var go = WeaponPrefab.Clone();
        var weapon = go.Components.Get<WeaponComponent>(true);
        var gaveItem = Inventory.TryGiveItem(weapon, ignorePickUpCD);
        return gaveItem;
    }

    public void TakeDamage(float damage, Vector3 position, Vector3 force, Vector3 normal, Guid attackerId, DamageType damageType = DamageType.BULLET, bool isHeadshot = false)
    {
        if (IsInvincible) return;
        Health -= damage;
        if (Health <= 0)
        {
            Health = 0;
            PlayerState = PlayerStates.DEAD;
            PlayerController.Instance.UpdateAnimatorOnDeath();
        }
    }

    /// <summary>
    /// Used to calculate score on kill
    /// </summary>
    /// <param name="isHeadshot">a headshot is an extra 10 points</param>
    /// <param name="mobScore">The score the mob rewards</param>
    public void OnKill(bool isHeadshot, int mobScore = 10)
    {
        TotalKills++;
        int finalScore = mobScore;
        if (isHeadshot) finalScore += HeadshotScoreIncrease;
        Score += finalScore;
    }

    public void AddXp(int amount)
    {
        if (PlayerState == PlayerStates.DEAD) return;
        CurXp += amount;
    }

    public void AddHealth(int amount)
    {
        if (PlayerState == PlayerStates.DEAD) return;

        Health += amount;
        if (Health > MaxHealth)
        {
            Health = MaxHealth;
        }
    }
}