namespace Kira;

[Group("Kira/Test")]
[Title("Test Loot Spawner")]
public sealed class TestLootSpawner : Component
{
    [Property, ResourceType("prefab")]
    public GameObject LootPrefab { get; set; }

    [Property, Range(0, 100)]
    public int SpawnAmount { get; set; } = 10;
    [Property] public string InputAction { get; set; } = "Slot1";

    [Property] private float SpawnCD { get; set; } = 0.1f;
    private TimeSince SpawnTimeSince { get; set; } = 0;

    protected override void OnUpdate()
    {
        base.OnUpdate();

        if (Input.Down(InputAction))
        {
            if (SpawnTimeSince > SpawnCD)
            {
                SpawnCubes();
                SpawnTimeSince = 0;
            }
        }
    }

    private async void SpawnCubes()
    {
        for (int i = 0; i < SpawnAmount; i++)
        {
            Vector3 spawnPos = Transform.Position + (Vector3.Random.WithZ(0.5f) * 20f);
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