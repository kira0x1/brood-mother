using System;

namespace Kira;

[Group("Kira/Mob"), Icon("man_3")]
public sealed class MobController : Component, IHealthComponent
{
    [Property] public float MaxHealth { get; private set; } = 100f;
    [Property] public float Health { get; private set; } = 100f;
    [Property] public float AttackSpeed { get; set; } = 0.4f;
    [Property] public float AttackRange { get; set; } = 10f; // is added to min distance
    [Property] public float AttackDamage { get; set; } = 5f; // is added to min distance
    [Property] public float MinDistance { get; private set; } = 60f;

    [Property] private MobDataResource MobData { get; set; }

    private NavMeshAgent Agent;
    private PlayerController Player;
    private AnimationController Animator;
    private SoundEvent HurtSound;
    private GameTransform Model;
    public Action<MobController> OnDeathEvent;
    private TimeSince NextAttackTime = 0;
    private float DistanceToPlayer;


    private enum MobStates
    {
        IDLE,
        CHASE,
        DEAD
    }

    private MobStates CurState { get; set; }


    protected override void OnAwake()
    {
        base.OnAwake();
        Agent = Components.Get<NavMeshAgent>();
        Animator = Components.Get<AnimationController>();
        if (MobData is not null)
            HurtSound = MobData.HurtSound;

        Model = Components.GetInChildren<ModelRenderer>().Transform;
    }

    protected override void OnStart()
    {
        base.OnStart();
        Player = PlayerController.Instance;
        Agent.MoveTo(Player.Transform.Position);
        CurState = MobStates.CHASE;
    }

    protected override void OnUpdate()
    {
        switch (CurState)
        {
            case MobStates.CHASE:
                ChaseState();
                break;
            case MobStates.IDLE:
                break;
            case MobStates.DEAD:
                break;
        }
    }

    private void ChaseState()
    {
        UpdateAnimator();

        DistanceToPlayer = Vector3.DistanceBetween(Transform.Position, Player.Transform.Position);

        if (DistanceToPlayer > MinDistance)
        {
            Agent.MoveTo(Player.Transform.Position);
        }
        else
        {
            Agent.MoveTo(Transform.Position);
        }

        HandleCombat();
        Model.Rotation = Rotation.FromYaw(Agent.Velocity.EulerAngles.ToRotation().Yaw());
    }

    private void HandleCombat()
    {
        // if close enough to hit player
        if (NextAttackTime > AttackSpeed && DistanceToPlayer <= MinDistance + AttackRange)
        {
            PlayerManager.Instance.TakeDamage(AttackDamage, Transform.Position, Transform.Local.Forward, GameObject.Id, DamageType.BLUNT);
            NextAttackTime = 0;
        }
    }

    private void UpdateAnimator()
    {
        // Animator.LookAtEnabled = false;
        // Animator.WithLook(Player.Transform.Position);
        Animator.WithVelocity(Agent.Velocity);
        Animator.WithWishVelocity(Agent.WishVelocity);
        // Transform.Rotation = Animator.EyeWorldTransform.Rotation;
    }

    public void TakeDamage(float damage, Vector3 position, Vector3 force, Guid attackerId,
                           DamageType damageType = DamageType.BULLET, bool isHeadshot = false)
    {
        if (damageType is DamageType.BULLET or DamageType.BLUNT)
        {
            var p = new SceneParticles(Scene.SceneWorld, "particles/impact.flesh.bloodpuff.vpcf");
            p.SetControlPoint(0, position);
            p.SetControlPoint(0, Rotation.LookAt(force.Normal * -1f));
            p.PlayUntilFinished(Task);
        }

        if (CurState == MobStates.DEAD)
        {
            return;
        }

        if (HurtSound is not null)
        {
            Sound.Play(HurtSound, Transform.Position);
        }

        Health -= damage;
        if (Health <= 0f)
        {
            Health = 0f;
            OnDeath(isHeadshot);
        }
    }

    private void OnDeath(bool headshot)
    {
        CurState = MobStates.DEAD;
        Agent.Stop();
        Animator.WithVelocity(Vector3.Zero);
        // Animator.Target.Enabled = false;
        PlayerManager.Instance.OnKill(headshot, MobData.ScoreReward);
        OnDeathEvent?.Invoke(this);
        // GameObject.Destroy();

        Animator.Sitting = AnimationController.SittingStyle.Floor;
        Animator.IsSitting = true;
    }
}