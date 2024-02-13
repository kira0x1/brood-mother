using Sandbox;

namespace Kira;

public sealed class WeaponComponent : Component
{
    [Property]
    public string WeaponName { get; set; }

    [Property] public WeaponResource WeaponResource { get; set; }

    public SkinnedModelRenderer Model { get; set; }

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
}