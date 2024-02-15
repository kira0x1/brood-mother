using System;
using System.Collections.Generic;
using Sandbox;

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
    private DecalRenderer CrosshairDecal { get; set; }
    private SkinnedModelRenderer Model { get; set; }
    private ParticleSystem MuzzleParticleSystem { get; set; }
    private SoundEvent ShootSound { get; set; }

    private readonly List<FromTo> arrows = new List<FromTo>();
    private readonly List<Vector3> hits = new List<Vector3>();


    protected override void OnAwake()
    {
        Model = Components.GetInDescendantsOrSelf<SkinnedModelRenderer>(true);
        MuzzleParticleSystem = Muzzle.Components.GetInChildren<ParticleSystem>(true);
        CrosshairDecal = Components.GetInDescendants<DecalRenderer>(true);

        WeaponName = WeaponData.Name;
        Spread = WeaponData.Spread;
        FireRate = WeaponData.FireRate;
        ShootSound = WeaponData.ShootSound;
        Damage = WeaponData.Damage;
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
            CrosshairDecal.Enabled = true;
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

        if (trace.Hit)
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
}