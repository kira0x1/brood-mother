using Sandbox;

namespace Kira;

public class Slot
{
    public string icon;
    public string title;
    public int id;
}

[Group("Kira")]
[Title("Player Manager")]
[Icon("person")]
public sealed class PlayerManager : Component
{
    [Property]
    public CharacterVitals Vitals { get; set; }

    [Property]
    public WeaponResource wep;

    public Slot[] Slots = new Slot[0];
    [Property]
    public int SlotActive;

    protected override void OnAwake()
    {
        base.OnAwake();
        Slots = new Slot[4];

        for (int i = 0; i < 4; i++)
        {
            var slot = new Slot();
            slot.title = $"{i + 1}";
            slot.id = i;
            if (i == 0)
                slot.icon = wep.Icon;
            Slots[i] = slot;
        }
    }

    protected override void OnStart()
    {
        base.OnStart();
        Log.Info(wep.Icon);
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        var ym = (int)Input.MouseWheel.y;
        if (ym == -1)
        {
            SlotActive--;
            if (SlotActive < 0) SlotActive = Slots.Length;
        }
        else if (ym == 1)
        {
            SlotActive++;
            if (SlotActive >= Slots.Length) SlotActive = 0;
        }
    }
}