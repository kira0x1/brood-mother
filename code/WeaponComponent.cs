using System;
using System.Collections.Generic;

namespace Kira;

public struct FromTo
{
    public Vector3 from;
    public Vector3 to;

    public FromTo(Vector3 from, Vector3 to)
    {
        this.from = from;
        this.to = to;
    }
}

public enum ShootTypes
{
    SINGLE,
    SHOTGUN,
    LASER
}

[Category("Kira/Weapon")]
public sealed class WeaponComponent : Component
{
    public string WeaponName { get; set; }
    public float FireRate { get; set; } = 2f;
    public WeaponManager Shooter { get; set; }

    [Property] public GameObject Muzzle { get; set; }
    [Property] public WeaponData WeaponData { get; set; }
    [Property] public GameObject WeaponProp { get; set; }

    [Group("Effects"), Property] public ParticleSystem MuzzleFlash { get; set; }
    [Group("Effects"), Property] public ParticleSystem MuzzleSmoke { get; set; }
    [Group("Effects"), Property] public GameObject ImpactEffect { get; set; }
    [Group("Effects"), Property] private GameObject DecalEffect { get; set; }

    [Group("Gizmos"), Property] private bool ShowArrowGizmos { get; set; } = false;
    [Group("Gizmos"), Property] private bool ShowHitGizmos { get; set; } = false;

    public Angles Recoil { get; set; }
    private float Spread { get; set; }
    private float Damage { get; set; }
    private float DamageForce { get; set; } = 10f;

    private DecalRenderer CrosshairDecal { get; set; }
    private SkinnedModelRenderer Model { get; set; }
    private ParticleSystem MuzzleParticleSystem { get; set; }
    private SoundEvent ShootSound { get; set; }
    private Transform MuzzleTransform;

    private readonly List<FromTo> arrows = new List<FromTo>();
    private readonly List<Vector3> hits = new List<Vector3>();

    private ShootTypes ShootType { get; set; } = ShootTypes.SINGLE;
    private int BulletsPerShot { get; set; } = 1;


    private WeaponAnimator Animator { get; set; }


    protected override void OnAwake()
    {
        Model = Components.GetInDescendantsOrSelf<SkinnedModelRenderer>(true);
        MuzzleParticleSystem = Muzzle.Components.GetInChildren<ParticleSystem>(true);
        CrosshairDecal = Components.GetInDescendants<DecalRenderer>(true);
        Animator = Components.Get<WeaponAnimator>();
        MuzzleTransform = new Transform(Muzzle.Transform.LocalPosition);

        WeaponName = WeaponData.Name;
        Spread = WeaponData.Spread;
        FireRate = WeaponData.FireRate;
        ShootSound = WeaponData.ShootSound;
        Damage = WeaponData.Damage;
        DamageForce = WeaponData.DamageForce;
        ShootType = WeaponData.ShootType;
        BulletsPerShot = WeaponData.BulletsPerShot;

        // if (ViewModel.IsValid())
        // {
        //     ViewModel.SetCamera(cam);
        //     ViewModel.SetWeaponComponent(this);
        // }

        base.OnAwake();
    }

    public void HolsterWeapon()
    {
        Model.Enabled = false;
        if (CrosshairDecal.IsValid())
        {
            CrosshairDecal.Enabled = false;
        }
    }

    public void DeployWeapon()
    {
        Model.Enabled = true;

        if (CrosshairDecal.IsValid())
        {
            if (PlayerController.Instance.ViewMode == ViewModes.FIRST_PERSON)
            {
                CrosshairDecal.Enabled = false;
            }
            else
            {
                CrosshairDecal.Enabled = true;
            }
        }
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        UpdateGizmos();
    }

    private void UpdateGizmos()
    {
        if (ShowArrowGizmos)
        {
            foreach (FromTo ft in arrows)
            {
                Gizmo.Draw.Arrow(ft.from, ft.to);
            }
        }

        if (ShowHitGizmos)
        {
            foreach (var pos in hits)
            {
                Gizmo.Draw.Color = Color.Green.WithAlpha(0.3f);
                Gizmo.Draw.LineSphere(pos, 5);
            }
        }
    }

    private SceneTraceResult GunTrace(float recoilModifier = 1f)
    {
        Vector3 startPos = Transform.Position;
        Vector3 direction = Muzzle.Transform.Rotation.Forward;

        if (PlayerController.Instance.ViewMode == ViewModes.FIRST_PERSON)
        {
            startPos = Scene.Camera.Transform.Position;
            direction = Scene.Camera.Transform.Rotation.Forward;
        }

        direction += Vector3.Random * (Spread * recoilModifier);

        Vector3 endPos = startPos + direction * 5000f;
        var trace = Scene.Trace.Ray(startPos, endPos)
            .IgnoreGameObjectHierarchy(GameObject.Root)
            .UsePhysicsWorld()
            .UseHitboxes()
            .Radius(3f)
            .Run();

        return trace;
    }

    private void BulletTrace(SceneTraceResult trace)
    {
        IHealthComponent damageable = null;
        if (trace.Component.IsValid()) damageable = trace.Component.Components.GetInAncestorsOrSelf<IHealthComponent>();

        float damage = WeaponData.Damage;
        if (damageable is not null)
        {
            var player = PlayerManager.Instance.WeaponManager;

            bool isHeadshot = false;
            if (trace.Hitbox is not null && trace.Hitbox.Tags.Has("head"))
            {
                isHeadshot = true;
                player.DoHitMarker(true);
                damage *= 3f;
            }
            else
            {
                player.DoHitMarker(false);
            }

            damageable.TakeDamage(damage, trace.EndPosition, trace.Direction * DamageForce, GameObject.Id, DamageType.BULLET, isHeadshot);
        }
        else if (trace.Hit)
        {
            hits.Add(trace.HitPosition);
            HandleHit(trace);
        }
        else
        {
            FromTo ft = new FromTo(trace.StartPosition, trace.EndPosition);
            arrows.Add(ft);
        }
    }

    public void Shoot()
    {
        MuzzleTransform = new Transform(Muzzle.Transform.Position, Muzzle.Transform.Rotation);

        if (MuzzleFlash is not null)
        {
            var p = new SceneParticles(Scene.SceneWorld, MuzzleFlash);
            p.SetControlPoint(0, MuzzleTransform);
            p.PlayUntilFinished(Task);
        }

        if (ShootType == ShootTypes.SINGLE)
        {
            var trace = GunTrace();

            HandleSound();
            HandleSmokeTrail(trace);
            BulletTrace(trace);
        }
        else if (ShootType == ShootTypes.SHOTGUN)
        {
            var firstTrace = GunTrace(0.25f);
            HandleSound();
            HandleSmokeTrail(firstTrace);
            BulletTrace(firstTrace);

            for (int i = 1; i < BulletsPerShot; i++)
            {
                //todo: maybe pass in 'i' and use as a modifier to spray around the bullets in a shotgun pattern
                var trace = GunTrace();
                HandleSmokeTrail(trace);
                BulletTrace(trace);
            }
        }
    }

    private void HandleSound()
    {
        if (ShootSound is not null)
        {
            Sound.Play(ShootSound, Muzzle.Transform.Position);
        }
    }

    private void HandleSmokeTrail(SceneTraceResult trace)
    {
        if (trace.Distance > 80f)
        {
            var p = new SceneParticles(Scene.SceneWorld, "particles/tracer/trail_smoke.vpcf");
            p.SetControlPoint(0, MuzzleTransform.Position);
            p.SetControlPoint(1, trace.EndPosition);
            p.SetControlPoint(2, trace.Distance);
            p.PlayUntilFinished(Task);
        }
    }

    private void HandleHit(SceneTraceResult trace)
    {
        var damageInfo = new DamageInfo(Damage, Shooter.GameObject, GameObject);

        foreach (var damageable in trace.GameObject.Components.GetAll<IDamageable>())
        {
            damageable.OnDamage(damageInfo);
        }

        var spawnPos = new Transform(trace.HitPosition + trace.Normal * 2.0f, Rotation.LookAt(-trace.Normal, Vector3.Random), Random.Shared.Float(0.8f, 1.2f));
        var decal = DecalEffect.Clone(spawnPos);
        ImpactEffect.Clone(spawnPos);
        decal.SetParent(trace.GameObject);
    }

    protected override void OnEnabled()
    {
        base.OnEnabled();
        PlayerController.Instance.OnViewModeChangedEvent += OnViewModeChanged;
    }

    protected override void OnDisabled()
    {
        base.OnDisabled();
        PlayerController.Instance.OnViewModeChangedEvent += OnViewModeChanged;
    }

    private void OnViewModeChanged(ViewModes viewMode)
    {
        if (!CrosshairDecal.IsValid()) return;
        if (viewMode == ViewModes.TOP_DOWN && Model.Enabled)
        {
            CrosshairDecal.Enabled = true;
        }
        else
        {
            CrosshairDecal.Enabled = false;
        }
    }

    public void OnAimChanged(bool isAiming)
    {
        if (!Animator.IsValid())
        {
            return;
        }

        Animator.OnAimChanged(isAiming);
        // Animator.OnDeploy();
    }
}