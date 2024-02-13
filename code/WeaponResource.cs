using Sandbox;

namespace Kira;

[GameResource("Weapon Data", "weapon", "Weapon Data", Icon = "🏹")]
public partial class WeaponResource : GameResource
{
    [Property]
    public string Name { get; set; }

    [Property, ResourceType("image")]
    public string Icon { get; set; }

    [Property]
    public float FireRate { get; set; }

    [Property]
    public AnimationController.HoldTypes WeaponHoldType { get; set; } = AnimationController.HoldTypes.None;
}