using System;
using Sandbox;

namespace Kira;

[Group("Kira")]
public class Inventory : Component
{
    public Slot[] Slots = Array.Empty<Slot>();
    public int PreviousSlot { get; set; }
    public int CurrentSlot { get; set; }
    public PlayerManager Player { get; set; }
    public AnimationController Animator { get; set; }

    protected override void OnAwake()
    {
        base.OnAwake();

        Player = Components.Get<PlayerManager>();
        Animator = Components.Get<AnimationController>();

        Slots = new Slot[4];

        for (int i = 0; i < 4; i++)
        {
            var slot = new Slot();
            slot.title = $"{i + 1}";
            slot.id = i;
            Slots[i] = slot;
        }
    }

    protected override void OnUpdate()
    {
        HandleScrolling();

        var weapon = ActiveWeapon;
        Animator.HoldType = ActiveSlot.hasItem && weapon != null ? weapon.WeaponHoldType : AnimationController.HoldTypes.None;

        if (Input.Pressed("Drop") && ActiveWeapon != null)
        {
            Player.weaponManager.HideWeapon();
            ActiveSlot.hasItem = false;
            ActiveSlot.icon = "";
        }
    }

    private void HandleScrolling()
    {
        var scroll = (int)Input.MouseWheel.y;

        if (scroll == 0)
        {
            return;
        }

        PreviousSlot = CurrentSlot;
        CurrentSlot -= scroll;
        if (CurrentSlot < 0) CurrentSlot = Slots.Length - 1;
        if (CurrentSlot >= Slots.Length) CurrentSlot = 0;
        OnSlotChanged();
    }

    private void OnSlotChanged()
    {
        var prevSlot = Slots[PreviousSlot];
        var curSlot = Slots[CurrentSlot];

        if (prevSlot.hasItem || !curSlot.hasItem)
        {
            Player.weaponManager.HideWeapon();
        }

        if (curSlot.hasItem)
        {
            Player.weaponManager.ShowWeapon();
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

    public Slot ActiveSlot => Slots[CurrentSlot];
    public WeaponResource ActiveWeapon => ActiveSlot.weapon;
}