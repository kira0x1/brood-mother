using System;

namespace Kira;

[Group("Kira/Player")]
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
    [Property, Group("Aim")] public GameObject Eye { get; set; }
    [Property, Group("Aim")] public bool IsAiming { get; set; }

    // [Property, Group("Aim")] public float MinLookY { get; set; } = -22f;
    // [Property, Group("Aim")] public float MaxLookY { get; set; } = 22f;


    public static PlayerController Instance { get; set; }


    public Angles EyeAngles { get; private set; }
    private WeaponManager WeaponManager;
    private RealTimeSince LastUngroundedTime;
    public CharacterController Controller;
    private AnimationController Animator;
    public Vector3 WishVelocity;
    private bool IsCrouching;
    private Angles Recoil { get; set; }
    private Inventory inventory { get; set; }

    private enum MoveModes
    {
        PLAYER_DIRECTION,
        WORLD_DIRECTION,
    }

    [Property, Group("Move")]
    private MoveModes MoveMode { get; set; }

    [Property, Group("Move")] public ViewModes ViewMode { get; set; }

    public Action<ViewModes> OnViewModeChangedEvent;

    protected override void OnAwake()
    {
        base.OnAwake();

        Instance = this;
        Animator = Components.GetInDescendantsOrSelf<AnimationController>();
        Controller = Components.GetInDescendantsOrSelf<CharacterController>();
        WeaponManager = Components.Get<WeaponManager>();
        inventory = Components.Get<Inventory>();

        if (Controller.IsValid())
        {
            Controller.Height = StandHeight;
        }

        ResetViewAngles();
        OnViewModeChanged();
    }

    protected override void OnPreRender()
    {
        base.OnPreRender();
        if (Eye.IsValid() && ViewMode == ViewModes.FIRST_PERSON && Game.InGame)
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
                .WithoutTags("weapon", "floor")
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
        // HandleCrouching();

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
        Transform.Rotation = Rotation.FromYaw(EyeAngles.ToRotation().Yaw());
    }

    protected override void OnUpdate()
    {
        UpdateRecoil();
        UpdateAnimator();
        HandleAiming();

        if (Input.Pressed("Voice"))
        {
            OnViewModeChanged();
        }
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
        var rotation = MoveMode == MoveModes.WORLD_DIRECTION ? Transform.LocalRotation : EyeAngles.ToRotation();
        // var rotation = Transform.LocalRotation;

        if (MoveMode == MoveModes.WORLD_DIRECTION)
        {
            WishVelocity = (Input.AnalogMove.Normal * MoveSpeed).WithZ(0f);
        }
        else
        {
            WishVelocity = rotation * Input.AnalogMove;
        }

        WishVelocity = WishVelocity.WithZ(0f);

        if (!WishVelocity.IsNearZeroLength)
            WishVelocity = WishVelocity.Normal;

        if (IsCrouching)
            WishVelocity *= 64f;
        else
            WishVelocity *= MoveSpeed;
    }

    private void OnViewModeChanged()
    {
        ViewMode = ViewMode == ViewModes.TOP_DOWN ? ViewModes.FIRST_PERSON : ViewModes.TOP_DOWN;

        if (ViewMode == ViewModes.TOP_DOWN)
        {
            MoveMode = MoveModes.WORLD_DIRECTION;
            Animator.Target.SetBodyGroup("head", 0);
            ResetViewAngles();
        }
        else
        {
            MoveMode = MoveModes.PLAYER_DIRECTION;
            Animator.Target.SetBodyGroup("head", 1);
        }

        OnViewModeChangedEvent?.Invoke(ViewMode);
    }

    private void HandleAiming()
    {
        if (IsAiming)
        {
            Animator.Target?.Set("aim_yaw", EyeAngles.pitch);
        }

        if (Input.Released("Attack2"))
        {
            IsAiming = !IsAiming;
            OnAimChanged();
        }
    }

    private void OnAimChanged()
    {
        if (!inventory.HasItem) return;

        var weapon = inventory.ActiveWeapon;
        if (!weapon.IsValid()) return;
        weapon.OnAimChanged(IsAiming);
    }
}