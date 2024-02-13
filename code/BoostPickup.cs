using System.Linq;
using Sandbox;


namespace Kira;

[Group("Kira")]
[Title("Boost Pickup")]
public sealed class BoostPickup : Component, Component.ITriggerListener
{
    [Property] private float Cooldown { get; set; } = 5f;
    [Property] private float SpeedBoost { get; set; } = 20f;
    [Property] private Color ActiveColor { get; set; } = Color.Green;
    [Property] private Color DisabledColor { get; set; } = Color.Red;
    [Property] private float Duration { get; set; } = 1f;
    [Property] private Collider Trigger { get; set; }
    [Property] public SoundEvent BoostSound { get; set; }

    private PointLight[] BoostLights { get; set; }
    private TimeSince lastUsed = 0;
    private bool CanPickup;

    protected override void OnStart()
    {
        lastUsed = Cooldown;
        base.OnStart();
    }

    protected override void OnAwake()
    {
        base.OnAwake();
        BoostLights = Components.GetAll<PointLight>().ToArray();
    }

    private void SetLightColor(Color color)
    {
        foreach (PointLight light in BoostLights)
        {
            light.LightColor = color;
        }
    }

    protected override void OnUpdate()
    {
        if (lastUsed.Relative < Cooldown) return;
        if (!CanPickup)
        {
            CanPickup = true;
            SetLightColor(ActiveColor);
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!CanPickup) return;
        if (!other.Tags.Has("player")) return;

        PlayerPickup player = other.Components.Get<PlayerPickup>();
        if (!player.IsValid()) return;

        Sound.Play(BoostSound, GameObject.Transform.Position);
        player.GiveSpeed(SpeedBoost, Duration, this.GetHashCode());

        SetLightColor(DisabledColor);
        lastUsed = 0;
        CanPickup = false;
    }

    public void OnTriggerExit(Collider other)
    {
        return;
    }
}