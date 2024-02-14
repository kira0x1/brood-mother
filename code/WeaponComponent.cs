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
    [Property]
    public string WeaponName { get; set; }

    [Property]
    public GameObject Muzzle { get; set; }

    [Property] public WeaponResource WeaponResource { get; set; }
    [Property] public float Spread { get; set; } = 0.01f;
    public SkinnedModelRenderer Model { get; set; }
    public List<FromTo> arrows = new List<FromTo>();

    [Property] public GameObject WeaponProp;

    protected override void OnAwake()
    {
        Model = Components.GetInDescendantsOrSelf<SkinnedModelRenderer>(true);
        base.OnAwake();
    }

    public void HolsterWeapon()
    {
        Model.Enabled = false;
    }

    public void DeployWeapon()
    {
        Model.Enabled = true;
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        foreach (FromTo ft in arrows)
        {
            Gizmo.Draw.Arrow(ft.from, ft.to);
        }
    }

    public SceneTraceResult GunTrace()
    {
        Vector3 startPos = Scene.Camera.Transform.Position;
        Vector3 direction = Scene.Camera.Transform.Rotation.Forward;
        direction += Vector3.Random * Spread;

        Vector3 endPos = startPos + direction * 10000f;
        var trace = Scene.Trace.Ray(startPos, endPos)
            .IgnoreGameObjectHierarchy(GameObject.Root)
            .UsePhysicsWorld()
            .UseHitboxes()
            .Run();

        return trace;
    }

    public void Shoot()
    {
        // var x = Scene.Trace.PhysicsTrace.FromTo(Muzzle.Transform.Position, Muzzle.Transform.Local.Forward * 100).Run();
        var trace = GunTrace();
        FromTo ft = new FromTo(Muzzle.Transform.Position, trace.HitPosition);
        arrows.Add(ft);

        if (trace.Hit)
        {
            Log.Info("HIT " + trace.Body.GetGameObject().Name);
        }
        else
        {
            // FromTo ft = new FromTo(Muzzle.Transform.Position, Muzzle.Transform.Local.Forward * 100);
            // arrows.Add(ft);
            Log.Info("NOTHING HIT");
        }
    }
}