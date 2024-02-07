using Sandbox;

public sealed class CameraController : Component
{
    [Property] public PlayerController PlayerController { get; set; }
    [Property] public float LerpSpeed { get; set; } = 5f;
    private Vector3 Offset;
    private GameTransform PlayerTransform;

    protected override void OnStart()
    {
        base.OnStart();
        PlayerTransform = PlayerController.Transform;
        Offset = PlayerTransform.Position - Transform.Position;
        Log.Info($"Offset: {Offset}");
    }

    protected override void OnUpdate()
    {
        var targetPos = PlayerTransform.Position - Offset;
        Transform.Position = Vector3.Lerp(Transform.Position, targetPos, LerpSpeed * Time.Delta);
    }
}