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
    [Group("Gizmos"), Property] private bool ShowHitGizmos { get; set; } = true;


    public Angles Recoil { get; set; }
    private float Spread { get; set; }
    private float Damage { get; set; }
    private float DamageForce { get; set; } = 10f;

    private DecalRenderer CrosshairDecal { get; set; }
    private SkinnedModelRenderer Model { get; set; }
    private ParticleSystem MuzzleParticleSystem { get; set; }
    private SoundEvent ShootSound { get; set; }
    private ViewModel ViewModel;
    private Transform MuzzleTransform;

    private readonly List<FromTo> arrows = new List<FromTo>();
    private readonly List<Vector3> hits = new List<Vector3>();


    protected override void OnAwake()
    {
        Model = Components.GetInDescendantsOrSelf<SkinnedModelRenderer>(true);
        MuzzleParticleSystem = Muzzle.Components.GetInChildren<ParticleSystem>(true);
        CrosshairDecal = Components.GetInDescendants<DecalRenderer>(true);
        ViewModel = Components.GetInDescendantsOrSelf<ViewModel>();

        MuzzleTransform = new Transform(Muzzle.Transform.LocalPosition);

        WeaponName = WeaponData.Name;
        Spread = WeaponData.Spread;
        FireRate = WeaponData.FireRate;
        ShootSound = WeaponData.ShootSound;
        Damage = WeaponData.Damage;
        DamageForce = WeaponData.DamageForce;

        var cam = Scene.Components.GetInDescendants<CameraComponent>();
        if (!cam.IsValid())
        {
            Log.Warning("camera not found");
        }

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

    private SceneTraceResult GunTrace()
    {
        Vector3 startPos = Transform.Position;
        Vector3 direction = Muzzle.Transform.Rotation.Forward;

        if (PlayerController.Instance.ViewMode == ViewModes.FIRST_PERSON)
        {
            startPos = Scene.Camera.Transform.Position;
            direction = Scene.Camera.Transform.Rotation.Forward;
        }

        direction += Vector3.Random * Spread;

        Vector3 endPos = startPos + direction * 5000f;
        var trace = Scene.Trace.Ray(startPos, endPos)
            .IgnoreGameObjectHierarchy(GameObject.Root)
            .UsePhysicsWorld()
            .UseHitboxes()
            .Radius(5f)
            .Run();

        return trace;
    }

    public void Shoot()
    {
        var trace = GunTrace();

        if (ShootSound is not null)
        {
            Sound.Play(ShootSound, Muzzle.Transform.Position);
        }

        MuzzleTransform = new Transform(Muzzle.Transform.Position, Muzzle.Transform.Rotation);

        if (trace.Distance > 80f)
        {
            var p = new SceneParticles(Scene.SceneWorld, "particles/tracer/trail_smoke.vpcf");
            p.SetControlPoint(0, MuzzleTransform.Position);
            p.SetControlPoint(1, trace.EndPosition);
            p.SetControlPoint(2, trace.Distance);
            p.PlayUntilFinished(Task);
        }

        if (MuzzleFlash is not null)
        {
            var p = new SceneParticles(Scene.SceneWorld, MuzzleFlash);
            p.SetControlPoint(0, MuzzleTransform);
            p.PlayUntilFinished(Task);
        }

        IHealthComponent damageable = null;
        if (trace.Component.IsValid())
            damageable = trace.Component.Components.GetInAncestorsOrSelf<IHealthComponent>();

        float damage = 1f;
        if (damageable is not null)
        {
            var player = PlayerManager.Instance.WeaponManager;

            if (trace.Hitbox is not null && trace.Hitbox.Tags.Has("head"))
            {
                Log.Info("on hit marker headshot");
                player.DoHitMarker(true);
                damage *= 3f;
            }
            else
            {
                Log.Info("on hit marker body");
                player.DoHitMarker(false);
            }

            damageable.TakeDamage(damage, trace.EndPosition, trace.Direction * DamageForce, GameObject.Id);
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
}