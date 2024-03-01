using System.Collections.Generic;

namespace Kira;

[GameResource("Mob Data", "mob", "Mob Data", Icon = "👻")]
public partial class MobDataResource : GameResource
{
    [Property] public string Name { get; set; } = "Mob";
    [Property, Group("Stats")] public float Health { get; set; } = 40;
    [Property, Group("Clothes")] public List<Outfit> Outfits { get; set; }

    [Property, Group("Reward")] public int ScoreReward { get; set; } = 10;
    [Property, Group("Spawn")] public int MinSpawnLevel { get; set; } = 1;
    [Property, Group("Spawn")] public int MaxSpawnLevel { get; set; } = 10;
    [Property, Group("Spawn")] public float SpawnRateByLevel { get; set; } = 1f;
    [Property, Group("Sound")] public SoundEvent HurtSound { get; set; }
}