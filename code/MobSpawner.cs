using System;
using System.Collections.Generic;
using System.Linq;

namespace Kira;

[Group("Kira/Mob")]
public sealed class MobSpawner : Component
{
    [Property]
    private bool SpawningOn { get; set; } = true;

    [Property, Group("Mobs"), Description("Does not spawn more mobs if there are this many mobs currently active in the scene")]
    private int MobLimit { get; set; } = 10;

    [Property, Group("Mobs"), ResourceType("prefab")]
    private List<GameObject> MobPrefabs { get; set; } = new List<GameObject>();

    [Property, Group("Cooldowns"), ValueRange(1f, 120f)] private Vector2 RandomSpawnTime { get; set; } = new Vector2(1f, 8f);
    [Property, Group("Cooldowns"), ValueRange(1, 120)] private Vector2 RandomWaitSpawnAmount { get; set; } = new Vector2(1f, 8f);
    [Property, Group("Cooldowns"), ValueRange(1f, 120f)] private Vector2 RandomWaitSpawnCooldown { get; set; } = new Vector2(1f, 8f);

    private int WaitAfterSpawningAmount { get; set; } = 2;
    private float WaitAfterSpawningTime { get; set; } = 10f;
    private TimeSince NextSpawnTime { get; set; } = 0;
    private TimeSince NextSpawnWaitTime { get; set; } = 0;

    private float SpawnCD;
    private int CurSpawnedSinceLastWait { get; set; }
    public int CurMobsAlive { get; private set; }
    private List<SpawnPoint> Spawners { get; set; } = new List<SpawnPoint>();
    private List<MobController> MobsSpawned { get; set; }
    private PlayerManager Player { get; set; }

    protected override void OnAwake()
    {
        base.OnAwake();

        MobsSpawned = new List<MobController>();
        Spawners = new List<SpawnPoint>();
        Spawners = Components.GetAll<SpawnPoint>(FindMode.InChildren).ToList();
        SpawnCD = GetRandomSpawnTime();
        NextSpawnWaitTime = WaitAfterSpawningTime;
    }

    protected override void OnStart()
    {
        base.OnStart();
        Player = PlayerManager.Instance;
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
        if (!SpawningOn || Player.PlayerState == PlayerManager.PlayerStates.DEAD) return;

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