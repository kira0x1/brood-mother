using System.Collections.Generic;

namespace Kira;

public class BoostTime
{
    public int hashCode;
    public bool isEnabled;
    public float speed;
    public float duration;
    public TimeSince timeElapsed;

    public BoostTime(float speed, float duration, int hashCode)
    {
        isEnabled = true;
        this.speed = speed;
        this.duration = duration;
        this.hashCode = hashCode;
        timeElapsed = 0;
    }
}

[Group("Kira/Player")]
[Title("Player Pickup")]
public sealed class PlayerPickup : Component
{
    [Property, Range(0, 1000)] private float PickUpRadius { get; set; } = 100f;
    [Property] private bool ShowRadiusGizmo { get; set; } = false;

    private float BaseSpeed { get; set; }
    private float CurrentSpeed { get; set; }
    private Dictionary<int, BoostTime> Boosts = new Dictionary<int, BoostTime>();
    private PlayerController Controller { get; set; }
    private PlayerManager PlayerManager { get; set; }


    protected override void OnStart()
    {
        Controller = Components.Get<PlayerController>();
        PlayerManager = Components.Get<PlayerManager>();

        if (Controller.IsValid())
        {
            BaseSpeed = Controller.MoveSpeed;
        }

        base.OnStart();
    }

    protected override void OnUpdate()
    {
        CurrentSpeed = CalculateSpeed();
        Controller.MoveSpeed = CurrentSpeed;
        HandlePickup();
    }

    protected override void DrawGizmos()
    {
        base.DrawGizmos();
        if (!ShowRadiusGizmo) return;
        Gizmo.Draw.Color = new Color(0.9f, 0f, 0.5f, 0.15f);
        Gizmo.Draw.LineSphere(new Vector3(0, 0, 0), PickUpRadius, 8);
    }


    private void HandlePickup()
    {
        var traces = Scene.Trace.Sphere(PickUpRadius, Transform.World.Position, Transform.World.Position).WithTag("loot").RunAll();
        foreach (var trace in traces)
        {
            if (trace.Hit)
            {
                LootCube loot = trace.GameObject.Components.Get<LootCube>();
                if (!loot.IsValid())
                {
                    Log.Warning($"No lootcube attached to {trace.GameObject.Name}");
                    return;
                }

                if (!loot.IsLooted)
                {
                    loot.Loot(this);
                }
            }
        }
    }

    private float CalculateSpeed()
    {
        float targetSpeed = BaseSpeed;

        foreach (BoostTime boost in Boosts.Values)
        {
            if (!boost.isEnabled) continue;

            if (boost.timeElapsed.Relative >= boost.duration)
            {
                CurrentSpeed -= boost.speed;
                boost.isEnabled = false;
            }

            targetSpeed += boost.speed;
        }

        return targetSpeed;
    }

    public void GiveSpeed(float speed, float duration, int hashCode)
    {
        if (Boosts.TryGetValue(hashCode, out BoostTime boostFound))
        {
            boostFound.timeElapsed = 0;
            boostFound.isEnabled = true;
            return;
        }

        Boosts.Add(hashCode, new BoostTime(speed, duration, hashCode));
    }

    public void OnLoot(LootCube loot)
    {
        Sound.Play("loot_sound", Transform.Position + Transform.Local.Forward * 500f);

        PlayerManager.Gold += loot.Gold;
        PlayerManager.Score += loot.Score;

        if (loot.Health > 0)
        {
            PlayerManager.AddHealth(loot.Health);
        }

        if (loot.Xp > 0)
        {
            PlayerManager.AddXp(loot.Xp);
        }


        loot.GameObject.Destroy();
    }
}