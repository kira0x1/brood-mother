using System;

namespace Kira;

public class Slot
{
    public string icon;
    public string title;
    public int id;

    public bool hasItem;
    public WeaponResource weapon;

    public void SetItem(WeaponResource weapon)
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