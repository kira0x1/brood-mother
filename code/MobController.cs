using System;

namespace Kira;

[Group("Kira/Mob"), Icon("man_3")]
public sealed class MobController : Component, IHealthComponent
{
    [Property] public float MaxHealth { get; private set; } = 100f;
    [Property] public float Health { get; private set; } = 100f;
    [Property] private MobDataResource MobData { get; set; }

    private NavMeshAgent Agent;
    private PlayerController Player;
    private AnimationController Animator;
    private SoundEvent HurtSound;

    public LifeStates LifeState { get; set; } = LifeStates.ALIVE;
    public Action<MobController> OnDeathEvent;

    [Property]
    public float MinDistance { get; } = 180f;


    protected override void OnAwake()
    {
        base.OnAwake();
        Agent = Components.Get<NavMeshAgent>();
        Animator = Components.Get<AnimationController>();
        if (MobData is not null)
            HurtSound = MobData.HurtSound;
    }

    protected override void OnStart()
    {
        base.OnStart();
        Player = PlayerController.Instance;
        Agent.MoveTo(Player.Transform.Position);
        Animator.LookAt = Player.GameObject;
    }

    protected override void OnUpdate()
    {
        Animator.LookAtEnabled = true;
        Animator.WithLook(Player.Transform.Position);
        Animator.WithVelocity(Agent.Velocity);
        Animator.WithWishVelocity(Agent.WishVelocity);
        // Transform.Rotation = Animator.EyeWorldTransform.Rotation;


        float distance = Vector3.DistanceBetween(Transform.Position, Player.Transform.Position);

        if (distance > MinDistance)
        {
            Agent.MoveTo(Player.Transform.Position);
        }
        else
        {
            Agent.MoveTo(Transform.Position);
        }
    }

    public void TakeDamage(float damage, Vector3 position, Vector3 force, Guid attackerId, DamageType damageType = DamageType.BULLET)
    {
        if (damageType == DamageType.BULLET || damageType == DamageType.BLUNT)
        {
            var p = new SceneParticles(Scene.SceneWorld, "particles/impact.flesh.bloodpuff.vpcf");
            p.SetControlPoint(0, position);
            p.SetControlPoint(0, Rotation.LookAt(force.Normal * -1f));
            p.PlayUntilFinished(Task);
        }

        if (LifeState == LifeStates.DEAD) return;

        if (HurtSound is not null)
        {
            Sound.Play(HurtSound, Transform.Position);
        }

        Health -= damage;
        if (Health <= 0f)
        {
            Health = 0f;
            OnDeath();
        }
    }

    private void OnDeath()
    {
        LifeState = LifeStates.DEAD;
        OnDeathEvent?.Invoke(this);
    }
}

public enum LifeStates
{
    ALIVE,
    DEAD
}