namespace Kira;

[Group("Kira")]
[Title("Loot Cube")]
public sealed class LootCube : Component
{
    [Property, Range(0, 15)] private float LerpSpeed { get; set; } = 5f;
    [Property, Range(0.1f, 150)] private float PickUpDistance { get; set; } = 10f;

    private bool IsLerping { get; set; }

    private PlayerPickup playerPickup;
    private GameTransform playerTransform;
    private Rigidbody rigidbody;
    private BoxCollider collider;

    public bool IsLooted { get; set; }

    [Property, Range(0, 50), Group("Loot")] public int Gold { get; set; } = 0;
    [Property, Range(0, 50), Group("Loot")] public int Xp { get; set; } = 0;
    [Property, Range(0, 50), Group("Loot")] public int Health { get; set; } = 0;
    [Property, Range(0, 50), Group("Loot")] public int Score { get; set; } = 0;

    protected override void OnStart()
    {
        base.OnStart();
        rigidbody = Components.Get<Rigidbody>();
        collider = Components.Get<BoxCollider>();
    }

    public void Loot(PlayerPickup player)
    {
        playerPickup = player;
        playerTransform = player.Transform;
        IsLerping = true;
        IsLooted = true;
        rigidbody.Enabled = false;
        collider.Enabled = false;
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        if (!IsLerping) return;

        var targetPos = playerTransform.Position.WithZ(40f);
        float distance = Vector3.DistanceBetween(Transform.Position, targetPos);

        if (distance < PickUpDistance)
        {
            PickUp();
        }

        Transform.Position = Vector3.Lerp(Transform.Position, targetPos, LerpSpeed * Time.Delta);
    }

    private void PickUp()
    {
        IsLerping = false;
        playerPickup.OnLoot(this);
    }
}