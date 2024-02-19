using System;
using Sandbox;

namespace Kira;

[Group("Kira/Player")]
[Title("Player Manager")]
[Icon("person")]
public sealed class PlayerManager : Component, IHealthComponent
{
    [Property]
    public float MaxHealth { get; private set; } = 100;

    [Property]
    public float Health { get; private set; } = 100;

    public AnimationController Animator { get; set; }
    public WeaponManager WeaponManager;
    public Inventory Inventory { get; set; }
    public static PlayerManager Instance { get; set; }

    protected override void OnAwake()
    {
        base.OnAwake();
        Instance = this;
        Animator = Components.Get<AnimationController>();
        Inventory = Components.Get<Inventory>();
        WeaponManager = GameObject.Components.Get<WeaponManager>();
    }

    public bool TryGiveItem(GameObject WeaponPrefab)
    {
        var go = WeaponPrefab.Clone();
        var weapon = go.Components.Get<WeaponComponent>(true);
        var gaveItem = Inventory.TryGiveItem(weapon);
        return gaveItem;
    }

    public void TakeDamage(float damage, Vector3 position, Vector3 force, Guid attackerId, DamageType damageType = DamageType.BULLET)
    {
        Health -= damage;
        if (Health <= 0)
        {
            Health = 0;
        }
    }
}