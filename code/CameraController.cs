namespace Kira;

public enum CameraFollowMode
{
    SMOOTH_FOLLOW,
    FIXED_FOLLOW
}

[Group("Kira/Player")]
[Title("Camera Controller")]
public sealed class CameraController : Component
{
    [Property] public PlayerController PlayerController { get; set; }
    [Property] public float LerpSpeed { get; set; } = 5f;

    [Group("Follow Mode"), Property] private Vector3 SmoothFollowOffset { get; set; }
    [Group("Follow Mode"), Property] private Vector3 FixedFollowOffset { get; set; }
    [Group("Follow Mode"), Property] public CameraFollowMode FollowMode { get; set; } = CameraFollowMode.FIXED_FOLLOW;

    private Vector3 Offset;
    private GameTransform PlayerTransform;
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
        PlayerTransform = PlayerController.Transform;
        Offset = PlayerTransform.Position - Transform.Position;

        switch (FollowMode)
        {
            case CameraFollowMode.FIXED_FOLLOW:
                break;
            case CameraFollowMode.SMOOTH_FOLLOW:
                GameObject.SetParent(Scene);
                break;
        }
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
        if (FollowMode != CameraFollowMode.SMOOTH_FOLLOW) return;
        var targetPos = PlayerTransform.Position - Offset;
        Transform.Position = Vector3.Lerp(Transform.Position, targetPos, LerpSpeed * Time.Delta);
    }

    protected override void OnFixedUpdate()
    {
        if (FollowMode != CameraFollowMode.FIXED_FOLLOW) return;
        Vector3 targetPos = PlayerTransform.Position - Offset;
        Transform.Position = targetPos;
    }

    public void OnViewModeChange(ViewModes vm)
    {
        if (vm == ViewModes.TOP_DOWN)
        {
            Transform.Rotation = StartRotation;
        }
    }
}