using Sandbox;

namespace Kira;

[Group("Kira")]
[Title("Player Controller")]
public sealed class PlayerController : Component
{
    [Property, Group("Move")] public float MoveSpeed { get; set; } = 80f;
    [Property, Group("Move")] public float TurnSpeed { get; set; } = 500f;
    [Property, Group("Move")] private Vector3 Gravity { get; set; } = new(0f, 0f, 800f);
    [Property, Group("Move")] private float JumpVelocity { get; set; } = 300f;
    [Property, Group("Move")] private float StandHeight { get; set; } = 64f;
    [Property, Group("Move")] private float CrouchHeight { get; set; } = 28f;

    // if current speed exceeds this then set anim to running
    [Property, Group("Move")] public float MinRunSpeed { get; set; } = 150f;
    [Property, Group("Aim")] private bool VerticalAimEnabled { get; set; } = false;
    [Property, Group("Aim")] public float AimSpeed { get; set; } = 2f;

    [Property] public GameObject Eye { get; set; }
    private Angles EyeAngles;
    private WeaponManager WeaponManager;
    private RealTimeSince LastUngroundedTime;
    private CharacterController Controller;
    private AnimationController Animator;
    private Vector3 WishVelocity;
    private bool IsCrouching;
    private Angles Recoil { get; set; }


    protected override void OnAwake()
    {
        base.OnAwake();

        Animator = Components.GetInDescendantsOrSelf<AnimationController>();
        Controller = Components.GetInDescendantsOrSelf<CharacterController>();
        WeaponManager = Components.Get<WeaponManager>();

        if (Controller.IsValid())
        {
            Controller.Height = StandHeight;
        }

        ResetViewAngles();
    }

    protected override void OnPreRender()
    {
        base.OnPreRender();
        if (Eye.IsValid())
        {
            var idealEyePos = Eye.Transform.Position;
            var headPosition = Transform.Position + Vector3.Up * Controller.Height;
            var headTrace = Scene.Trace.Ray(Transform.Position, headPosition)
                .UsePhysicsWorld()
                .IgnoreGameObjectHierarchy(GameObject)
                .Run();

            headPosition = headTrace.EndPosition - headTrace.Direction * 2f;

            var trace = Scene.Trace.Ray(headPosition, idealEyePos)
                .UsePhysicsWorld()
                .IgnoreGameObjectHierarchy(GameObject)
                .WithAnyTags("solid")
                .Radius(2f)
                .Run();

            Scene.Camera.Transform.Position = trace.Hit ? trace.EndPosition : idealEyePos;
            Scene.Camera.Transform.Rotation = EyeAngles.ToRotation() * Rotation.FromPitch(-10f);
        }
    }

    protected override void OnFixedUpdate()
    {
        HandleMove();
    }

    private void HandleMove()
    {
        BuildWishVelocity();

        // var inputVal = Input.AnalogMove.Normal.WithZ(0f);
        // var fwd = Transform.LocalRotation;
        // var finalVal = (inputVal * fwd * MoveSpeed).WithZ(0f);
        // WishVelocity = (Input.AnalogMove.Normal * MoveSpeed).WithZ(0f);
        // WishVelocity = finalVal;

        HandleCrouching();

        if (Controller.IsOnGround && Input.Down("Jump"))
        {
            Controller.Punch(Vector3.Up * JumpVelocity);
            Animator.TriggerJump();
        }

        if (Controller.IsOnGround)
        {
            Controller.Velocity = Controller.Velocity.WithZ(0f);
            Controller.Accelerate(WishVelocity);
            Controller.ApplyFriction(4.0f);
        }
        else
        {
            Controller.Velocity -= Gravity * Time.Delta * 0.5f;
            Controller.Accelerate(WishVelocity.ClampLength(50f));
            Controller.ApplyFriction(0.1f);
        }

        Controller.Move();

        if (!Controller.IsOnGround)
        {
            Controller.Velocity -= Gravity * Time.Delta * 0.5f;
            LastUngroundedTime = 0f;
        }
        else
        {
            Controller.Velocity = Controller.Velocity.WithZ(0);
        }


        UpdateRotation();
    }

    private void HandleCrouching()
    {
        if (!Input.Pressed("Duck") || !Controller.IsOnGround) return;

        // Toggle Crouching
        if (!IsCrouching)
        {
            Controller.Height = CrouchHeight;
            IsCrouching = true;
        }
        else
        {
            if (!CanUncrouch()) return;
            Controller.Height = StandHeight;
            IsCrouching = false;
        }
    }

    private bool CanUncrouch()
    {
        if (!IsCrouching) return true;
        if (LastUngroundedTime < 0.2f) return false;

        var cc = GameObject.Components.Get<CharacterController>();
        var tr = cc.TraceDirection(Vector3.Up * CrouchHeight);
        return !tr.Hit;
    }

    private void UpdateRotation()
    {
        // float turnAxis = Input.AnalogLook.yaw * TurnSpeed * Time.Delta;
        // Transform.LocalRotation = Transform.LocalRotation.RotateAroundAxis(Vector3.Up, turnAxis);
        Transform.Rotation = Rotation.FromYaw(EyeAngles.ToRotation().Yaw());
    }

    protected override void OnUpdate()
    {
        UpdateRecoil();
        UpdateAnimator();
    }

    private void UpdateAnimator()
    {
        Animator.IsGrounded = Controller.IsOnGround;
        Animator.MoveStyle = MoveSpeed >= MinRunSpeed ? AnimationController.MoveStyles.Run : AnimationController.MoveStyles.Walk;
        Animator.DuckLevel = IsCrouching ? 1f : 0f;
        Animator.FootShuffle = 0f;

        Animator.WithVelocity(Controller.Velocity);
        Animator.WithWishVelocity(WishVelocity);
        Animator.WithLook(EyeAngles.Forward);
        Animator.UpdateIk();
    }

    private void UpdateRecoil()
    {
        var angles = EyeAngles.Normal;
        angles += Input.AnalogLook * 0.5f;
        angles += Recoil * Time.Delta;
        angles.pitch = angles.pitch.Clamp(-60f, 80f);
        EyeAngles = angles.WithRoll(0f);
    }

    public void ApplyRecoil(Angles recoil)
    {
        Recoil += recoil;
    }

    public void ResetViewAngles()
    {
        var rotation = Rotation.Identity;
        EyeAngles = rotation.Angles().WithRoll(0f);
    }

    private void BuildWishVelocity()
    {
        var rotation = EyeAngles.ToRotation();
        // var rotation = Transform.LocalRotation;
        WishVelocity = rotation * Input.AnalogMove;
        WishVelocity = WishVelocity.WithZ(0f);

        if (!WishVelocity.IsNearZeroLength)
            WishVelocity = WishVelocity.Normal;

        if (IsCrouching)
            WishVelocity *= 64f;
        else
            WishVelocity *= MoveSpeed;
    }

    private void HandleEyes()
    {
        if (!VerticalAimEnabled) return;
        var ee = EyeAngles;
        ee += Input.AnalogLook * AimSpeed;
        ee.roll = 0;
        EyeAngles = ee;

        Animator.WithLook(EyeAngles.Forward, 1, 1, 1.0f);
    }
}