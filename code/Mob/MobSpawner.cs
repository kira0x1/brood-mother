using System;
using System.Collections.Generic;
using System.Linq;

namespace Kira;

[Group("Kira/Mob")]
[Title("Mob Spawner"), Icon("manage_accounts")]
public sealed class MobSpawner : Component
{
    [Property]
    private bool SpawningOn { get; set; } = true;

    [Property, Group("Spawning"), Description("Does not spawn more mobs if there are this many mobs currently active in the scene")]
    private int MobLimit { get; set; } = 0;

    [Property, Group("Spawning"), Description("Does not spawn more if there are this many alive")]
    private int MobAliveLimit { get; set; } = 10;

    [Property, Group("Mobs"), ResourceType("prefab")]
    private List<GameObject> MobPrefabs { get; set; } = new List<GameObject>();

    [Description("The time inbetween spawning each indivisual mob")]
    [Property, Group("Cooldowns"), ValueRange(1f, 120f)] private Vector2 MobSpawnDelay { get; set; } = new Vector2(1f, 8f);

    [Description("How many mobs to spawn for each group")]
    [Property, Group("Cooldowns"), ValueRange(1, 120)] private Vector2 MobsInGroupAmount { get; set; } = new Vector2(1f, 8f);

    [Description("After spawning `MobsToSpawnAmount` of mobs wait for n seconds")]
    [Property, Group("Cooldowns"), ValueRange(1f, 120f)] private Vector2 MobGroupSpawnDelay { get; set; } = new Vector2(1f, 8f);

    public int CurMobsAlive { get; private set; }
    private List<SpawnPoint> Spawners { get; set; } = new List<SpawnPoint>();
    private List<MobController> MobsSpawned { get; set; }
    private PlayerManager Player { get; set; }
    private bool doneSpawningGroup = true;


    protected override void OnAwake()
    {
        base.OnAwake();

        MobsSpawned = new List<MobController>();
        Spawners = new List<SpawnPoint>();
        Spawners = Components.GetAll<SpawnPoint>(FindMode.InChildren).ToList();
    }

    protected override void OnStart()
    {
        base.OnStart();
        Player = PlayerManager.Instance;
    }


    protected override void OnUpdate()
    {
        if (!SpawningOn || Player.PlayerState == PlayerManager.PlayerStates.DEAD) return;

        if (doneSpawningGroup)
        {
            if (MobLimit > 0 && MobsSpawned.Count >= MobLimit) return;
            if (MobAliveLimit > 0 && CurMobsAlive >= MobAliveLimit) return;

            var mobsAmount = GetRandMobAmount();
            SpawnMobGroup(mobsAmount, GetRandGroupSpawnDelay());
        }
    }

    private async void SpawnMobGroup(int spawnAmount, float waitDelaySeconds)
    {
        doneSpawningGroup = false;

        var mobToSpawn = GetRandomMob();
        for (int i = 0; i < spawnAmount; i++)
        {
            float delay = GetRandSpawnDelay();
            await Task.DelaySeconds(delay);
            SpawnMob(mobToSpawn);
        }

        await Task.DelaySeconds(waitDelaySeconds);
        doneSpawningGroup = true;
    }


    private void SpawnMob(GameObject mobPrefab)
    {
        GameObject mobGo = mobPrefab.Clone(GetRandomSpawnPoint().Transform.Position);
        OnMobSpawned(mobGo);
    }

    private void OnMobSpawned(GameObject mobGo)
    {
        mobGo.BreakFromPrefab();
        MobController mob = mobGo.Components.Get<MobController>();

        if (!mob.IsValid())
        {
            Log.Warning($"Failed to find MobController in {mobGo.Name}");
            return;
        }

        mob.OnDeathEvent += OnMobDeath;
        MobsSpawned.Add(mob);
        CurMobsAlive++;
    }

    private void OnMobDeath(MobController mob)
    {
        CurMobsAlive--;
    }

    private SpawnPoint GetRandomSpawnPoint()
    {
        return Random.Shared.FromList(Spawners);
    }

    private GameObject GetRandomMob()
    {
        return Random.Shared.FromList(MobPrefabs);
    }

    private float GetRandSpawnDelay()
    {
        return Random.Shared.Float(MobSpawnDelay.x, MobSpawnDelay.y);
    }

    private int GetRandMobAmount()
    {
        return Random.Shared.Int((int)MobsInGroupAmount.x, (int)MobsInGroupAmount.y);
    }

    private float GetRandGroupSpawnDelay()
    {
        return Random.Shared.Float(MobGroupSpawnDelay.x, MobGroupSpawnDelay.y);
    }
}