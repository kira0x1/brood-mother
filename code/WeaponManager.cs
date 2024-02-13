using System.Collections.Generic;
using Sandbox;

namespace Kira;

public sealed class WeaponManager : Component
{
    [Property]
    public GameObject WeaponBone;
    public GameObject ActiveWeapon;
    public WeaponComponent Weapon;
    private AnimationController Animator;

    protected override void OnAwake()
    {
        base.OnAwake();
        Animator = Components.Get<AnimationController>();
    }

    public void OnGiveWeapon(WeaponComponent weapon)
    {
        HideWeapon();

        ActiveWeapon = weapon.GameObject;
        ActiveWeapon.SetParent(WeaponBone);
        ActiveWeapon.Transform.Position = WeaponBone.Transform.Position;
        ActiveWeapon.Transform.Rotation = WeaponBone.Transform.Rotation;
        ActiveWeapon.Enabled = true;
        Weapon = weapon;
        weapon.DeployWeapon();
    }

    public void HideWeapon()
    {
        if (ActiveWeapon.IsValid())
        {
            ActiveWeapon.Enabled = false;
        }

        Animator.HoldType = AnimationController.HoldTypes.None;
    }

    public void ShowWeapon()
    {
        if (ActiveWeapon.IsValid()) ActiveWeapon.Enabled = true;
        if (Weapon.IsValid())
            Animator.HoldType = Weapon.WeaponResource.WeaponHoldType;
    }
}