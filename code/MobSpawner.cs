using System;
using System.Collections.Generic;
using System.Linq;

namespace Kira;

[Group("Kira/Mob")]
public sealed class MobSpawner : Component
{
    [Property, ValueRange(1f, 120f)] private Vector2 RandomSpawnTime { get; set; } = new Vector2(1f, 8f);
    [Property, ResourceType("prefab")] private List<GameObject> MobPrefabs { get; set; } = new List<GameObject>();

    [Property, Description("Does not spawn more mobs if there are this many mobs currently active in the scene")] private int MobLimit = 10;

    private int WaitAfterSpawningAmount { get; set; } = 2;
    private float WaitAfterSpawningTime { get; set; } = 10f;

    [Property, Group("Cooldowns"), ValueRange(1, 120)] private Vector2 RandomWaitSpawnAmount { get; set; } = new Vector2(1f, 8f);
    [Property, Group("Cooldowns"), ValueRange(1f, 120f)] private Vector2 RandomWaitSpawnCooldown { get; set; } = new Vector2(1f, 8f);


    private TimeSince NextSpawnTime { get; set; } = 0;
    private TimeSince NextSpawnWaitTime { get; set; } = 0;

    private float SpawnCD;
    private int CurSpawnedSinceLastWait { get; set; }
    private List<SpawnPoint> Spawners { get; set; } = new List<SpawnPoint>();
    private List<MobController> MobsSpawned { get; set; }
    public int CurMobsAlive { get; set; } = 0;

    protected override void OnAwake()
    {
        base.OnAwake();

        MobsSpawned = new List<MobController>();
        Spawners = new List<SpawnPoint>();
        Spawners = Components.GetAll<SpawnPoint>(FindMode.InChildren).ToList();
        SpawnCD = GetRandomSpawnTime();
        NextSpawnWaitTime = WaitAfterSpawningTime;
    }

    private float GetRandomSpawnTime()
    {
        return Random.Shared.Float(RandomSpawnTime.x, RandomSpawnTime.y);
    }

    private int GetRandomWaitAmount()
    {
        return Random.Shared.Int((int)RandomWaitSpawnAmount.x, (int)RandomWaitSpawnAmount.y);
    }

    private float GetRandomWaitCooldown()
    {
        return Random.Shared.Float(RandomWaitSpawnCooldown.x, RandomWaitSpawnCooldown.y);
    }

    protected override void OnUpdate()
    {
        if (NextSpawnTime > SpawnCD && NextSpawnWaitTime > WaitAfterSpawningTime)
        {
            if (CurMobsAlive < MobLimit)
                SpawnMob();
        }
    }

    private void SpawnMob()
    {
        CurSpawnedSinceLastWait++;
        SpawnCD = GetRandomSpawnTime();
        WaitAfterSpawningAmount = GetRandomWaitAmount();
        WaitAfterSpawningTime = GetRandomWaitCooldown();

        NextSpawnTime = 0;
        var point = GetRandomSpawnPoint();
        GameObject mobGo = GetRandomMob().Clone(point.Transform.Position);
        MobController mob = mobGo.Components.Get<MobController>();
        mob.OnDeathEvent += OnMobDeath;
        MobsSpawned.Add(mob);
        CurMobsAlive++;

        if (CurSpawnedSinceLastWait >= WaitAfterSpawningAmount)
        {
            NextSpawnWaitTime = 0;
            CurSpawnedSinceLastWait = 0;
        }
    }

    private void OnMobDeath(MobController mob)
    {
        CurMobsAlive--;
        // Log.Info("mob died");
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