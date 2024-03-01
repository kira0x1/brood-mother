namespace Kira;

[Group("Kira/Player")]
[Title("Camera Controller")]
public sealed class CameraController : Component
{
    [Property] public PlayerController PlayerController { get; set; }
    [Property] private float FollowSpeed { get; set; } = 5f;
    [Property] private float LookSpeed { get; set; } = 10f;

    private Rotation StartRotation { get; set; }

    public static CameraController Instance;

    protected override void OnAwake()
    {
        base.OnAwake();
        Instance = this;
    }

    protected override void OnStart()
    {
        base.OnStart();
        StartRotation = Transform.Rotation;
    }

    protected override void OnEnabled()
    {
        base.OnEnabled();
        PlayerController.Instance.OnViewModeChangedEvent += OnViewModeChange;
    }

    protected override void OnDisabled()
    {
        base.OnDisabled();
        PlayerController.Instance.OnViewModeChangedEvent += OnViewModeChange;
    }

    protected override void OnUpdate()
    {
        // if (FollowMode != CameraFollowMode.SMOOTH_FOLLOW) return;
        // var targetPos = PlayerTransform.Position - Offset;
        // Transform.Position = Vector3.Lerp(Transform.Position, targetPos, LerpSpeed * Time.Delta);
    }

    public void SetAngles(Vector3 pos, Rotation rot)
    {
        Transform.Position = Vector3.Lerp(Transform.Position, pos, FollowSpeed * Time.Delta);
        Transform.Rotation = Rotation.Lerp(Transform.Rotation, rot, LookSpeed * Time.Delta);
    }

    public void OnViewModeChange(ViewModes vm)
    {
        if (vm == ViewModes.TOP_DOWN)
        {
            Transform.Rotation = StartRotation;
        }
    }
}