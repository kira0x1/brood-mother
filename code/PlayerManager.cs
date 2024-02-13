using Sandbox;

namespace Kira;

[Group("Kira")]
[Title("Player Manager")]
[Icon("person")]
public sealed class PlayerManager : Component
{
    [Property]
    public CharacterVitals Vitals { get; set; }

    public WeaponManager weaponManager;
    public Inventory Inventory { get; set; }

    [Property]
    public WeaponResource crowbar;
    private AnimationController Animator;

    protected override void OnAwake()
    {
        base.OnAwake();
        Animator = GameObject.Components.Get<AnimationController>();
        Inventory = Components.Get<Inventory>();
        weaponManager = GameObject.Components.Get<WeaponManager>();
    }

    public bool TryGiveItem(GameObject WeaponPrefab)
    {
        var go = WeaponPrefab.Clone();
        var weapon = go.Components.Get<WeaponComponent>(true);
        var gaveItem = Inventory.TryGiveItem(weapon.WeaponResource);

        if (gaveItem)
        {
            weaponManager.OnGiveWeapon(weapon);
        }

        return gaveItem;
    }
}