using System.Collections.Generic;
using System.Linq;
using Sandbox;

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

[Group("Kira")]
[Title("Player Pickup")]
public sealed class PlayerPickup : Component
{
    private float BaseSpeed { get; set; }
    private float CurrentSpeed { get; set; }
    private Dictionary<int, BoostTime> Boosts = new Dictionary<int, BoostTime>();
    private PlayerController Controller { get; set; }

    protected override void OnStart()
    {
        Controller = Components.Get<PlayerController>();
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
}