using Sandbox;

namespace Kira;

[Category("Kira")]
public sealed class WeaponPickup : Component, Component.ITriggerListener
{
    [Property]
    public WeaponResource Weapon { get; set; }

    [Property]
    public GameObject WeaponPrefab { get; set; }


    public void OnTriggerEnter(Collider other)
    {
        if (!other.Tags.Has("player")) return;

        PlayerManager player = other.Components.Get<PlayerManager>();
        if (!player.IsValid()) return;

        if (player.TryGiveItem(WeaponPrefab))
        {
            GameObject.Destroy();
        }
    }

    public void OnTriggerExit(Collider other)
    {
    }
}