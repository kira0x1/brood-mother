using Sandbox;

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
    public float FireRate { get; set; } = 0.1f;

    [Property]
    public float Spread { get; set; } = 0.01f;

    [Property]
    public SoundEvent ShootSound { get; set; }

    [Property]
    public AnimationController.HoldTypes WeaponHoldType { get; set; } = AnimationController.HoldTypes.None;
}