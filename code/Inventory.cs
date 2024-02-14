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

    private TimeSince timeSinceLastPickup { get; set; }

    [Property]
    private float PickupCooldown { get; set; } = 1.0f;

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
        HandleSlotInput();

        var weapon = ActiveWeapon;
        Animator.HoldType = ActiveSlot.hasItem && weapon != null ? weapon.WeaponHoldType : AnimationController.HoldTypes.None;

        if (Input.Pressed("Drop") && ActiveSlot.hasItem)
        {
            OnWeaponDrop(Player.weaponManager.Weapon);
            Player.weaponManager.HideWeapon();
            ActiveSlot.hasItem = false;
            ActiveSlot.icon = "";
        }
    }

    // Spawn prop
    private void OnWeaponDrop(WeaponComponent weapon)
    {
        if (!weapon.IsValid())
        {
            Log.Warning("Weapon Not Valid");
            return;
        }

        if (!weapon.WeaponProp.IsValid())
        {
            Log.Warning("Weapon Prop is not valid!");
            return;
        }

        var pos = Transform.Position + Transform.LocalRotation.Forward * 50f;
        var prop = weapon.WeaponProp.Clone(pos);
        prop.BreakFromPrefab();
    }

    private void HandleSlotInput()
    {
        if (Input.Pressed("Slot1"))
        {
            SelectSlot(0);
        }
        else if (Input.Pressed("Slot2"))
        {
            SelectSlot(1);
        }
        else if (Input.Pressed("Slot3"))
        {
            SelectSlot(2);
        }
        else if (Input.Pressed("Slot4"))
        {
            SelectSlot(3);
        }
    }

    public void SelectSlot(int slotId)
    {
        PreviousSlot = CurrentSlot;
        CurrentSlot = slotId;
        OnSlotChanged();
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
        if (timeSinceLastPickup < PickupCooldown)
        {
            return false;
        }

        for (var i = 0; i < Slots.Length; i++)
        {
            Slot slot = Slots[i];

            if (!slot.hasItem)
            {
                timeSinceLastPickup = 0;
                slot.SetItem(weapon);
                PreviousSlot = CurrentSlot;
                CurrentSlot = i;
                OnSlotChanged();
                return true;
            }
        }

        return false;
    }

    public Slot ActiveSlot => Slots[CurrentSlot];
    public WeaponResource ActiveWeapon => ActiveSlot.weapon;
}