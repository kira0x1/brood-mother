namespace Kira;

[Category("Kira/Weapon")]
public sealed class GunAnimator : Component
{
    [Property] public SkinnedModelRenderer ModelRenderer { get; set; }
    private CharacterController Controller;
    private PlayerController playerController;

    private GameObject Camera;

    protected override void OnStart()
    {
        base.OnStart();

        Camera = CameraController.Instance.GameObject;

        if (!ModelRenderer.IsValid())
        {
            ModelRenderer = Components.Get<SkinnedModelRenderer>();
        }

        playerController = PlayerController.Instance;
        Controller = playerController.Controller;
    }

    public void OnDeploy()
    {
        ModelRenderer.Set("2H_Deploy_Safety", true);
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        LocalRotation = Rotation.Identity;
        LocalPosition = Vector3.Zero;

        ApplyVelocity();
        // ApplyStates();
        ApplyAnimationParameters();

        LerpedLocalRotation = Rotation.Lerp(LerpedLocalRotation, LocalRotation, Time.Delta * 10f);
        LerpedLocalPosition = LerpedLocalPosition.LerpTo(LocalPosition, Time.Delta * 10f);

        Camera.Transform.Position = Camera.Transform.Position;
        Camera.Transform.Rotation = Camera.Transform.Rotation;

        Transform.LocalRotation = LerpedLocalRotation;
        Transform.LocalPosition = LerpedLocalPosition;
    }


    private void ApplyAnimationParameters()
    {
        // ModelRenderer.Set("b_sprint", UseSprintAnimation && PlayerController.MoveSpeed > PlayerController.MinRunSpeed);
        ModelRenderer.Set("b_grounded", Controller.IsOnGround);

        // Ironsights
        ModelRenderer.Set("ironsights", PlayerController.Instance.IsAiming ? 2 : 0);
        ModelRenderer.Set("ironsights_fire_scale", PlayerController.Instance.IsAiming ? 0.3f : 0f);

        if (playerController.IsAiming)
        {
            ModelRenderer?.Set("aim_yaw", playerController.EyeAngles.yaw);
            ModelRenderer?.Set("aim_pitch", playerController.EyeAngles.pitch);
        }
    }

    private void ApplyStates()
    {
        LocalPosition += Vector3.Backward * 2f;
        LocalRotation *= Rotation.From(10f, 25f, -5f);
    }

    private Vector3 LerpedWishLook { get; set; }
    private Vector3 LocalPosition { get; set; }
    private Rotation LocalRotation { get; set; }
    private Vector3 LerpedLocalPosition { get; set; }
    private Rotation LerpedLocalRotation { get; set; }

    private void ApplyVelocity()
    {
        var moveVel = Controller.Velocity;
        var moveLen = moveVel.Length;

        var wishLook = playerController.WishVelocity.Normal * 1f;
        if (playerController.IsAiming) wishLook = 0;

        LerpedWishLook = LerpedWishLook.LerpTo(wishLook, Time.Delta * 5.0f);

        LocalRotation *= Rotation.From(0, -LerpedWishLook.y * 3f, 0);
        LocalPosition += -LerpedWishLook;

        ModelRenderer.Set("move_groundspeed", moveLen);
    }

    public void OnAimChanged(bool isAiming)
    {
        // ModelRenderer.Set("ironsights", isAiming ? 2 : 0);
        // ModelRenderer.Set("ironsights_fire_scale", isAiming ? 0.3f : 0f);
        // ModelRenderer.Set("b_deploy", isAiming);
        // ModelRenderer.Set("b_attack", isAiming);
    }
}