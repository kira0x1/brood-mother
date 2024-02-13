using System;
using Sandbox;

namespace Kira;

public class Slot
{
    public string icon;
    public string title;
    public int id;

    public bool hasItem;
    public WeaponResource weapon;

    public void SetItem(WeaponResource weapon)
    {
        this.weapon = weapon;
        this.icon = weapon.Icon;
        this.title = weapon.Name;
        this.hasItem = true;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(hasItem, weapon, hasItem);
    }
}

[Group("Kira")]
[Title("Player Manager")]
[Icon("person")]
public sealed class PlayerManager : Component
{
    [Property]
    public CharacterVitals Vitals { get; set; }

    public WeaponManager weaponManager;
    public Inventory Inventory { get; set; }

    [Property]
    public WeaponResource crowbar;

    private AnimationController Animator;

    protected override void OnAwake()
    {
        base.OnAwake();
        Animator = GameObject.Components.Get<AnimationController>();
        weaponManager = GameObject.Components.Get<WeaponManager>();
        Inventory = new Inventory();
        Inventory.Init();
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        Inventory.Update();

        var weapon = Inventory.ActiveWeapon;
        Animator.HoldType = Inventory.ActiveSlot.hasItem && weapon != null ? weapon.WeaponHoldType : AnimationController.HoldTypes.None;
    }

    public bool TryGiveItem(GameObject WeaponPrefab)
    {
        var go = WeaponPrefab.Clone();
        var weapon = go.Components.Get<WeaponComponent>();
        var gaveItem = Inventory.TryGiveItem(weapon.WeaponResource);

        if (gaveItem)
        {
            // var pos = Animator.Transform.Position;
            // var clone = new GameObject();
            // clone.Transform.Position = pos;
            // clone.SetParent(Animator.Transform.GameObject);
            // var mr = clone.Components.Create<ModelRenderer>();
            // mr.Model = item.Model;

            weaponManager.OnGiveWeapon(weapon);
            // weaponManager.ShowWeapon();
            // Animator.TriggerDeploy();
        }

        return gaveItem;
    }
}