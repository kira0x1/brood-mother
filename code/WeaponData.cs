namespace Kira;

[GameResource("Weapon Data", "weapon", "Weapon Data", Icon = "🏹")]
public partial class WeaponData : GameResource
{
    [Property]
    public string Name { get; set; }

    [Property, ResourceType("image")]
    public string Icon { get; set; }

    [Property]
    public float Damage { get; set; } = 10f;

    [Property]
    public float DamageForce { get; set; } = 5f;

    [Property]
    public float FireRate { get; set; } = 0.1f;

    [Property]
    public float Spread { get; set; } = 0.01f;

    [Property, Range(0.01f, 1f, 0.01f)]
    public Angles Recoil { get; set; } = new Angles(0f, 1f, 0f);

    [Property]
    public SoundEvent ShootSound { get; set; }

    [Property]
    public AnimationController.HoldTypes WeaponHoldType { get; set; } = AnimationController.HoldTypes.None;

    [Property]
    public ShootTypes ShootType { get; set; } = ShootTypes.SINGLE;

    [Property, ShowIf(nameof(ShootType), ShootTypes.SHOTGUN), Range(0, 20, 1)]
    public int BulletsPerShot { get; set; } = 1;

    [Property, ShowIf(nameof(ShootType), ShootTypes.SHOTGUN), Range(0.01f, 1f, 0.01f)]
    public float ShotgunSpread { get; set; } = 0.1f;
}