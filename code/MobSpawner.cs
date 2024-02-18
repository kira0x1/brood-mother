using System;
using System.Collections.Generic;
using System.Linq;

namespace Kira;

[Group("Kira/Mob")]
public sealed class MobSpawner : Component
{
    [Property, ValueRange(1f, 120f)] private Vector2 RandomSpawnTime { get; set; } = new Vector2(1f, 8f);
    [Property, ResourceType("prefab")] private List<GameObject> MobPrefabs { get; set; } = new List<GameObject>();

    private List<SpawnPoint> Spawners { get; set; } = new List<SpawnPoint>();
    private List<MobController> MobsSpawned { get; set; }

    private TimeSince NextSpawnTime { get; set; } = 0;
    private float SpawnCD;

    protected override void OnAwake()
    {
        base.OnAwake();

        MobsSpawned = new List<MobController>();
        Spawners = new List<SpawnPoint>();
        Spawners = Components.GetAll<SpawnPoint>(FindMode.InChildren).ToList();
        SpawnCD = GetRandomSpawnTime();
    }

    private float GetRandomSpawnTime()
    {
        return Random.Shared.Float(RandomSpawnTime.x, RandomSpawnTime.y);
    }

    protected override void OnUpdate()
    {
        if (NextSpawnTime > SpawnCD)
        {
            SpawnMob();
        }
    }

    private void SpawnMob()
    {
        Log.Info("spawning mob");
        SpawnCD = GetRandomSpawnTime();
        NextSpawnTime = 0;
        var point = GetRandomSpawnPoint();
        GameObject mobGo = GetRandomMob().Clone(point.Transform.Position);
        MobController mob = mobGo.Components.Get<MobController>();
        mob.OnDeathEvent += OnMobDeath;
        MobsSpawned.Add(mob);
    }

    private void OnMobDeath(MobController mob)
    {
        Log.Info("mob died");
    }

    public SpawnPoint GetRandomSpawnPoint()
    {
        return Random.Shared.FromList(Spawners);
    }

    private GameObject GetRandomMob()
    {
        return Random.Shared.FromList(MobPrefabs);
    }
}