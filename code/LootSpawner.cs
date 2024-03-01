namespace Kira;

[Group("Kira")]
[Title("Loot Spawner")]
public sealed class LootSpawner : Component
{
    [Property, ResourceType("prefab")]
    public GameObject LootPrefab { get; set; }

    [Property, Range(0, 100)]
    public int SpawnAmount { get; set; } = 10;

    public bool HasSpawnedLoot { get; set; } = false;

    public void SpawnLoot()
    {
        SpawnCubes();
        HasSpawnedLoot = true;
    }

    private async void SpawnCubes()
    {
        for (int i = 0; i < SpawnAmount; i++)
        {
            Vector3 spawnPos = Transform.Position + (Vector3.Random.WithZ(1.5f) * 20f);
            var cube = SpawnCube(spawnPos);
            await Task.Delay(10);
        }
    }

    private LootCube SpawnCube(Vector3 spawnPos)
    {
        var go = LootPrefab.Clone(spawnPos);
        go.BreakFromPrefab();
        LootCube cube = go.Components.Get<LootCube>();
        return cube;
    }
}