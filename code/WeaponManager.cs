using System.Collections.Generic;
using Sandbox;

namespace Kira;

public sealed class WeaponManager : Component
{
    [Property]
    public GameObject WeaponBone;

    private AnimationController Animator;

    protected override void OnAwake()
    {
        base.OnAwake();
        Animator = Components.Get<AnimationController>();
    }

    public void OnGiveWeapon(WeaponComponent weapon)
    {
        weapon.GameObject.SetParent(WeaponBone);
        weapon.GameObject.Transform.Position = WeaponBone.Transform.Position;
        weapon.GameObject.Transform.Rotation = WeaponBone.Transform.Rotation;
        weapon.DeployWeapon();
    }

    public void HideWeapon()
    {
    }

    protected override void OnUpdate()
    {
    }
}