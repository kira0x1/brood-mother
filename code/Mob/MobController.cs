using System;

// ReSharper disable SwitchStatementMissingSomeEnumCasesNoDefault

namespace Kira;

[Group("Kira/Mob"), Icon("man_3")]
public sealed class MobController : Component, IHealthComponent
{
    [Property] public float MaxHealth { get; private set; } = 100f;
    [Property] public float Health { get; private set; } = 100f;
    [Property] public float AttackSpeed { get; set; } = 0.4f;
    [Property] public float AttackRange { get; set; } = 10f; // is added to min distance
    [Property] public float AttackDamage { get; set; } = 5f; // is added to min distance
    [Property] public float KeepDistanceToPlayer { get; private set; } = 60f;
    [Property] private GameObject BulletDecal { get; set; }


    [Property] private MobDataResource MobData { get; set; }

    private NavMeshAgent Agent;
    private Rigidbody rigidbody { get; set; }
    private PlayerController Player;
    private AnimationController Animator;
    private SoundEvent HurtSound;
    private GameTransform Model;
    public Action<MobController> OnDeathEvent;
    private TimeSince NextAttackTime = 0;
    private float DistanceToPlayer;
    private ClothingRenderer Clothing { get; set; }
    private ModelPhysics ModelPhys { get; set; }

    private enum MobStates
    {
        IDLE,
        CHASE,
        DEAD
    }

    private enum DeathModes
    {
        DISSAPEAR,
        SIT,
        RAGDOLL
    }

    private DeathModes DeathMode { get; set; } = DeathModes.RAGDOLL;
    private MobStates CurState { get; set; }

    private bool DissapearAfterTime { get; set; } = true;
    private float DissapearDelay { get; set; } = 5;
    private TimeSince DissapearTime { get; set; }


    protected override void OnAwake()
    {
        base.OnAwake();
        Agent = Components.Get<NavMeshAgent>();
        Animator = Components.Get<AnimationController>();
        Clothing = Components.Get<ClothingRenderer>();

        if (MobData is not null)
        {
            if (Clothing.IsValid())
            {
                if (!Clothing.ApplyClothesOnStart)
                {
                    Clothing.SetOutfits(MobData.Outfits);
                    Clothing.ApplyClothing();
                }
            }

            HurtSound = MobData.HurtSound;
        }

        Model = Components.GetInChildren<ModelRenderer>().Transform;
        ModelPhys = Model.GameObject.Components.Get<ModelPhysics>(true);
        rigidbody = Model.GameObject.Components.Get<Rigidbody>(true);
    }

    protected override void OnStart()
    {
        base.OnStart();
        Player = PlayerController.Instance;
        Agent.MoveTo(Player.Transform.Position);
        CurState = MobStates.CHASE;
        Animator.FaceOverride = AnimationController.FaceOverrides.Angry;
    }

    protected override void OnUpdate()
    {
        UpdateAnimator();

        switch (CurState)
        {
            case MobStates.CHASE:
                ChaseState();
                break;
            case MobStates.IDLE:
                break;
            case MobStates.DEAD:
                if (DissapearAfterTime && DissapearTime > DissapearDelay)
                {
                    GameObject.Destroy();
                }

                break;
        }
    }

    private void ChaseState()
    {
        DistanceToPlayer = Vector3.DistanceBetween(Transform.Position, Player.Transform.Position);

        if (DistanceToPlayer > KeepDistanceToPlayer)
        {
            Agent.MoveTo(Player.Transform.Position);
            Model.Rotation = Rotation.FromYaw(Agent.Velocity.EulerAngles.ToRotation().Yaw());
        }
        else
        {
            Agent.MoveTo(Transform.Position);
        }

        HandleCombat();
    }

    private void HandleCombat()
    {
        // if close enough to hit player
        if (NextAttackTime > AttackSpeed && DistanceToPlayer <= AttackRange)
        {
            PlayerManager.Instance.TakeDamage(AttackDamage, Transform.Position, Transform.Local.Forward, Vector3.Zero, GameObject.Id, DamageType.BLUNT);
            NextAttackTime = 0;
        }
    }

    private void UpdateAnimator()
    {
        Animator.LookAtEnabled = true;
        Animator.WithLook(-Player.Transform.Local.Forward);


        switch (CurState)
        {
            case MobStates.CHASE:
                Animator.WithVelocity(Agent.Velocity);
                Animator.WithWishVelocity(Agent.WishVelocity);
                break;
            case MobStates.DEAD:
                Animator.WithVelocity(rigidbody.Velocity);
                Animator.WithWishVelocity(rigidbody.AngularVelocity);
                break;
        }

        // Transform.Rotation = Animator.EyeWorldTransform.Rotation;
    }

    public void TakeDamage(float damage, Vector3 position, Vector3 normal, Vector3 force, Guid attackerId,
                           DamageType damageType = DamageType.BULLET, bool isHeadshot = false)

    {
        var spawnPos = new Transform(position + normal * 2.0f, Rotation.LookAt(-normal, Vector3.Random), Random.Shared.Float(0.8f, 1.2f));
        var forceToLocal = -spawnPos.PointToLocal(Transform.Position);

        if (damageType is DamageType.BULLET or DamageType.BLUNT)
        {
            var p = new SceneParticles(Scene.SceneWorld, "particles/impact.flesh.bloodpuff.vpcf");
            p.SetControlPoint(0, position);
            p.SetControlPoint(0, Rotation.LookAt(force.Normal * -1f));
            p.PlayUntilFinished(Task);


            if (BulletDecal.IsValid())
            {
                var decal = BulletDecal.Clone(spawnPos);
                decal.SetParent(GameObject);
            }

            // var weapon = Scene.Directory.FindByGuid(attackerId);
            // var attacker = weapon.Parent;
            Animator.ProceduralHitReaction(new DamageInfo(), damage * 10, forceToLocal);
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
            OnDeath(isHeadshot, forceToLocal);
        }
    }

    private void OnDeath(bool headshot, Vector3 force = new Vector3())
    {
        DissapearTime = 0;
        CurState = MobStates.DEAD;
        Agent.Stop();
        Animator.WithVelocity(Vector3.Zero);

        PlayerManager.Instance.OnKill(headshot, MobData.ScoreReward);
        OnDeathEvent?.Invoke(this);
        Agent.Enabled = false;

        LootSpawner lootSpawner = Components.Get<LootSpawner>();
        if (lootSpawner.IsValid())
        {
            if (!lootSpawner.HasSpawnedLoot) lootSpawner.SpawnLoot();
        }


        switch (DeathMode)
        {
            case DeathModes.SIT:
                Animator.Sitting = AnimationController.SittingStyle.Floor;
                Animator.IsSitting = true;
                break;
            case DeathModes.DISSAPEAR:
                // Animator.Target.Enabled = false;
                GameObject.Destroy();
                break;
            case DeathModes.RAGDOLL:
                Animator.LookAt = Player.GameObject;
                Animator.LookAtEnabled = true;
                ModelPhys.Enabled = true;
                // rigidbody.Enabled = true;
                Tags.Set("ragdoll", true);
                Animator.ProceduralHitReaction(new DamageInfo(), 100f, force * 100000f);
                break;
        }
    }
}