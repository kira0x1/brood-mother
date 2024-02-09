using Sandbox;


namespace Kira;

[Group("Kira")]
[Title("Boost Pickup")]
public sealed class BoostPickup : Component
{
    [Property]
    public float Cooldown { get; set; } = 5f;

    [Property]
    public float SpeedBoost { get; set; } = 20f;

    [Property]
    public float Duration { get; set; } = 1f;

    [Property]
    private Collider Trigger { get; set; }

    private TimeSince lastUsed = 0;

    int pickupCount;

    protected override void OnStart()
    {
        lastUsed = Cooldown;
        base.OnStart();
    }

    protected override void OnUpdate()
    {
        if (lastUsed.Relative < Cooldown) return;

        foreach (Collider collider in Trigger.Touching)
        {
            if (collider.Tags.Has("player"))
            {
                Log.Info($"PICKUP {pickupCount}");
                Log.Info("--------------------");
                pickupCount++;
                PlayerPickup player = collider.Components.Get<PlayerPickup>();
                if (!player.IsValid()) break;
                player.GiveSpeed(SpeedBoost, Duration, this.GetHashCode());
                lastUsed = 0;
                break;
            }
        }
    }
}