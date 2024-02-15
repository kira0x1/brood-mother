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

    public override int ComponentVersion => 1;

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

        if (!ActiveSlot.hasItem)
        {
            Animator.HoldType = AnimationController.HoldTypes.None;

            return;
        }

        Animator.HoldType = ActiveWeapon.WeaponData.WeaponHoldType;

        if (Input.Pressed("Drop") && ActiveSlot.hasItem)
        {
            DropWeapon(Player.WeaponManager.Weapon);
            Player.WeaponManager.DropWeapon(ActiveSlot.id);

            // Clear Inventory Slot
            ActiveSlot.hasItem = false;
            ActiveSlot.icon = "";
        }
    }

    // Spawn prop
    private void DropWeapon(WeaponComponent weapon)
    {
        if (!weapon.IsValid() || !weapon.WeaponProp.IsValid())
        {
            return;
        }

        var pos = Transform.Position + Transform.LocalRotation.Forward * 50f;
        var prop = weapon.WeaponProp.Clone(pos);

        // doing this just so i can inspect the object in the scene, withought having to open the prefab in the editor
        // probably better to comment this out when done
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

        if (prevSlot.hasItem)
        {
            Player.WeaponManager.HideWeapon(prevSlot.id);
        }

        if (curSlot.hasItem)
        {
            Player.WeaponManager.ShowWeapon(curSlot.id);
        }
    }

    public new int GetHashCode => HashCode.Combine(Slots[0].GetHashCode(), Slots[1].GetHashCode(), Slots[2].GetHashCode(), Slots[3].GetHashCode());

    public bool TryGiveItem(WeaponComponent weapon)
    {
        if (timeSinceLastPickup < PickupCooldown)
        {
            return false;
        }

        if (!ActiveSlot.hasItem)
        {
            GiveItemToSlot(ActiveSlot.id, weapon);
            return true;
        }

        for (var i = 0; i < Slots.Length; i++)
        {
            Slot slot = Slots[i];

            if (!slot.hasItem)
            {
                timeSinceLastPickup = 0;
                PreviousSlot = CurrentSlot;
                CurrentSlot = i;
                GiveItemToSlot(CurrentSlot, weapon);
                return true;
            }
        }

        return false;
    }

    private void GiveItemToSlot(int slotId, WeaponComponent weapon)
    {
        Slots[slotId].SetItem(weapon);
        Player.WeaponManager.OnGiveWeapon(weapon, slotId);
    }

    public Slot ActiveSlot => Slots[CurrentSlot];
    public WeaponComponent ActiveWeapon => ActiveSlot.Weapon;
}