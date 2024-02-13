using System;
using Sandbox;

namespace Kira;

public class Inventory
{
    public Slot[] Slots = Array.Empty<Slot>();
    [Property]
    public int SlotActive;

    public void Init()
    {
        Slots = new Slot[4];

        for (int i = 0; i < 4; i++)
        {
            var slot = new Slot();
            slot.title = $"{i + 1}";
            slot.id = i;
            Slots[i] = slot;
        }
    }

    public void Update()
    {
        HandleScrolling();

        if (Input.Pressed("Drop") && ActiveWeapon != null)
        {
            ActiveSlot.hasItem = false;
            ActiveSlot.icon = "";
        }
    }

    private void HandleScrolling()
    {
        var ym = (int)Input.MouseWheel.y;
        if (ym == 1)
        {
            SlotActive--;
            if (SlotActive < 0) SlotActive = Slots.Length - 1;
        }
        else if (ym == -1)
        {
            SlotActive++;
            if (SlotActive >= Slots.Length) SlotActive = 0;
        }
    }

    public new int GetHashCode => HashCode.Combine(Slots[0].GetHashCode(), Slots[1].GetHashCode(), Slots[2].GetHashCode(), Slots[3].GetHashCode());

    public bool TryGiveItem(WeaponResource weapon)
    {
        foreach (Slot slot in Slots)
        {
            if (!slot.hasItem)
            {
                slot.SetItem(weapon);
                return true;
            }
        }

        return false;
    }

    public Slot ActiveSlot => Slots[SlotActive];
    public WeaponResource ActiveWeapon => ActiveSlot.weapon;
}