using Sandbox;


namespace Kira;

[Group("Kira")]
[Title("Boost Pickup")]
public sealed class BoostPickup : Component
{
    [Property] private float Cooldown { get; set; } = 5f;
    [Property] private float SpeedBoost { get; set; } = 20f;
    [Property] private Color ActiveColor { get; set; } = Color.Green;
    [Property] private Color DisabledColor { get; set; } = Color.Red;
    [Property] private float Duration { get; set; } = 1f;
    [Property] private Collider Trigger { get; set; }
    private PointLight BoostLight { get; set; }
    private TimeSince lastUsed = 0;
    private bool CanPickup;

    int pickupCount;

    protected override void OnStart()
    {
        lastUsed = Cooldown;
        base.OnStart();
    }

    protected override void OnAwake()
    {
        base.OnAwake();
        BoostLight = Components.GetInDescendantsOrSelf<PointLight>();
        BoostLight.LightColor = ActiveColor;
    }

    protected override void OnUpdate()
    {
        if (lastUsed.Relative < Cooldown) return;
        if (!CanPickup)
        {
            CanPickup = true;
            BoostLight.LightColor = ActiveColor;
        }

        foreach (Collider collider in Trigger.Touching)
        {
            if (collider.Tags.Has("player"))
            {
                pickupCount++;
                PlayerPickup player = collider.Components.Get<PlayerPickup>();
                if (!player.IsValid()) break;
                player.GiveSpeed(SpeedBoost, Duration, this.GetHashCode());
                lastUsed = 0;
                BoostLight.LightColor = DisabledColor;
                CanPickup = false;
                break;
            }
        }
    }
}