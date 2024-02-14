using Sandbox;

namespace Kira;

[Category("Kira")]
public sealed class WeaponPickup : Component, Component.ITriggerListener
{
    [Property]
    public WeaponResource Weapon { get; set; }

    [Property]
    public GameObject WeaponPrefab { get; set; }

    [Property]
    public bool hasPickedUp { get; set; } = false;

    public bool PlayerInTrigger { get; set; } = false;
    private PlayerManager player;

    private void OnPickUp(PlayerManager player)
    {
        if (player.TryGiveItem(WeaponPrefab))
        {
            PlayerInTrigger = false;
            hasPickedUp = true;
            GameObject.Destroy();
        }
    }

    protected override void OnUpdate()
    {
        if (hasPickedUp || !PlayerInTrigger) return;

        if (Input.Pressed("use"))
        {
            if (player.IsValid())
                OnPickUp(player);
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (hasPickedUp) return;
        if (!other.Tags.Has("player")) return;

        if (!player.IsValid())
        {
            player = other.Components.Get<PlayerManager>();
        }

        PlayerInTrigger = true;
    }

    public void OnTriggerExit(Collider other)
    {
        PlayerInTrigger = false;
    }
}