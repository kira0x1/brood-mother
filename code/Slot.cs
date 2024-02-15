using System;

namespace Kira;

public class Slot
{
    public string icon;
    public string title;
    public int id;

    public bool hasItem;
    public WeaponData weaponData => Weapon.WeaponData;
    public WeaponComponent Weapon;

    public void SetItem(WeaponComponent weapon)
    {
        this.Weapon = weapon;
        this.icon = weaponData.Icon;
        this.title = weaponData.Name;
        this.hasItem = true;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(hasItem, Weapon, hasItem);
    }
}