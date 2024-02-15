using Sandbox;

namespace Kira;

[Group("Kira")]
[Title("Player Manager")]
[Icon("person")]
public sealed class PlayerManager : Component
{
    [Property]
    public CharacterVitals Vitals { get; set; }

    public AnimationController Animator { get; set; }
    public WeaponManager WeaponManager;
    public Inventory Inventory { get; set; }

    protected override void OnAwake()
    {
        base.OnAwake();
        Animator = Components.Get<AnimationController>();
        Inventory = Components.Get<Inventory>();
        WeaponManager = GameObject.Components.Get<WeaponManager>();
    }

    public bool TryGiveItem(GameObject WeaponPrefab)
    {
        var go = WeaponPrefab.Clone();
        var weapon = go.Components.Get<WeaponComponent>(true);
        var gaveItem = Inventory.TryGiveItem(weapon);
        return gaveItem;
    }
}