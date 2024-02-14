using System;

namespace Kira;

public class Slot
{
    public string icon;
    public string title;
    public int id;

    public bool hasItem;
    public WeaponData weapon;

    public void SetItem(WeaponData weapon)
    {
        this.weapon = weapon;
        this.icon = weapon.Icon;
        this.title = weapon.Name;
        this.hasItem = true;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(hasItem, weapon, hasItem);
    }
}