using Sandbox;
using Sandbox.Diagnostics;

namespace Kira;

[Group("Kira")]
[Title("Player Shoot")]
public sealed class PlayerShoot : Component
{
    [Property] private float FireRate { get; set; } = 0.1f;
    [Property] private GameObject Prefab { get; set; }
    [Property] private GameObject AimIK { get; set; }
    private TimeSince LastShootTime = 0;

    PlayerManager player;

    protected override void OnAwake()
    {
        base.OnAwake();
        player = Components.Get<PlayerManager>();
    }

    protected override void OnUpdate()
    {
        Assert.NotNull(Prefab);

        if (!player.Inventory.ActiveSlot.hasItem)
        {
            return;
        }

        var weapon = player.weaponManager.Weapon;
        if (!weapon.IsValid()) return;

        if (Input.Down("Attack1") && LastShootTime > FireRate)
        {
            LastShootTime = 0;
            weapon.Shoot();
        }
    }

    private void Shoot()
    {
        GameObject clone = Prefab.Clone(AimIK.Transform.Position);
        var rb = clone.Components.Get<Rigidbody>();
        rb.Velocity = (Vector3.Up * 180) + Transform.Local.Forward * 480f;
    }
}