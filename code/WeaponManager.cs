namespace Kira;

[Group("Kira/Weapon")]
[Title("Weapon Manager")]
public sealed class WeaponManager : Component
{
    public RealTimeSince LastHitmarkerTime { get; private set; }
    [Property] private GameObject WeaponBone { get; set; }

    public Angles Recoil
    {
        get
        {
            if (Weapon.IsValid()) return Weapon.Recoil;
            return Angles.Zero;
        }
    }

    public WeaponComponent Weapon => SpawnedWeapons[ActiveWeapon];
    private TimeSince LastShootTime = 0;
    private int ActiveWeapon;
    private AnimationController Animator;
    private PlayerManager Player;
    private PlayerController Controller;
    private WeaponComponent[] SpawnedWeapons = new WeaponComponent[4];

    protected override void OnAwake()
    {
        base.OnAwake();
        Player = Components.Get<PlayerManager>();
        Animator = Components.Get<AnimationController>();
        Controller = Components.Get<PlayerController>();
    }

    protected override void OnFixedUpdate()
    {
        if (!Player.Inventory.HasItem || Player.PlayerState == PlayerManager.PlayerStates.DEAD)
        {
            return;
        }

        if (!Weapon.IsValid()) return;

        if (Input.Down("Attack1") && LastShootTime > Weapon.FireRate)
        {
            Player.Animator.Target?.Set("b_attack", true);
            LastShootTime = 0;
            Weapon.Shoot();
            Controller.ApplyRecoil(Recoil);
        }
    }

    public void DoHitMarker(bool isHeadshot)
    {
        Sound.Play(isHeadshot ? "hitmarker.headshot" : "hitmarker.hit");
        LastHitmarkerTime = 0f;
    }

    public void OnGiveWeapon(WeaponComponent weapon, int slotId)
    {
        if (ActiveWeapon != slotId && Weapon.IsValid())
        {
            HideWeapon(ActiveWeapon);
        }

        HideWeapon(slotId);

        var weaponGo = weapon.GameObject;
        weaponGo.SetParent(WeaponBone);
        weaponGo.Transform.Position = WeaponBone.Transform.Position;
        weaponGo.Transform.Rotation = WeaponBone.Transform.Rotation;
        weaponGo.Enabled = true;

        SpawnedWeapons[slotId] = weapon;
        ActiveWeapon = slotId;
        Weapon.Shooter = this;
        weapon.DeployWeapon();
    }

    public void OnDropWeapon(int slotId)
    {
        var weapon = SpawnedWeapons[slotId];
        weapon.GameObject.Destroy();
        Animator.HoldType = AnimationController.HoldTypes.None;
    }

    public void HideWeapon(int slotId)
    {
        var weapon = SpawnedWeapons[slotId];

        if (weapon.IsValid())
        {
            weapon.HolsterWeapon();
        }

        Animator.HoldType = AnimationController.HoldTypes.None;
    }

    public void ShowWeapon(int slotId)
    {
        var weapon = SpawnedWeapons[slotId];

        if (!weapon.IsValid())
        {
            return;
        }

        ActiveWeapon = slotId;
        weapon.DeployWeapon();
        Animator.HoldType = Weapon.WeaponData.WeaponHoldType;
    }

    public void OnAimChanged(bool isAiming)
    {
        Player.Animator.Target?.Set("aim", true);
    }
}