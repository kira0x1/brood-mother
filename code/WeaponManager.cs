using Sandbox;

namespace Kira;

[Group("Kira")]
[Title("Weapon Manager")]
public sealed class WeaponManager : Component
{
    [Property] public GameObject WeaponBone;

    public WeaponComponent Weapon;
    private TimeSince LastShootTime = 0;
    private GameObject ActiveWeapon;
    private AnimationController Animator;
    private PlayerManager Player;

    protected override void OnAwake()
    {
        base.OnAwake();
        Player = Components.Get<PlayerManager>();
        Animator = Components.Get<AnimationController>();
    }

    protected override void OnUpdate()
    {
        if (!Player.Inventory.ActiveSlot.hasItem)
        {
            return;
        }

        if (!Weapon.IsValid()) return;

        if (Input.Down("Attack1") && LastShootTime > Weapon.FireRate)
        {
            Player.Animator.Target?.Set("b_attack", true);
            LastShootTime = 0;
            Weapon.Shoot();
        }
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
        Weapon.Shooter = this;
        weapon.DeployWeapon();
    }

    public void DropWeapon()
    {
        if (ActiveWeapon.IsValid())
        {
            ActiveWeapon.Destroy();
        }

        Animator.HoldType = AnimationController.HoldTypes.None;
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
            Animator.HoldType = Weapon.WeaponData.WeaponHoldType;
    }
}