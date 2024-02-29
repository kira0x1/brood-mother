using System;
using System.Linq;

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

    [Property, Group("Look")] private bool VerticalAimEnabled { get; set; } = false;
    [Property, Group("Look")] public float AimSpeed { get; set; } = 2f;
    [Property, Group("Look")] public GameObject Eye { get; set; }
    [Property, Group("Look")] public bool IsAiming { get; set; }

    // [Property, Group("Aim")] public float MinLookY { get; set; } = -22f;
    // [Property, Group("Aim")] public float MaxLookY { get; set; } = 22f;


    public static PlayerController Instance { get; set; }

    public Angles EyeAngles { get; set; }
    private PlayerManager playerManager;
    private RealTimeSince LastUngroundedTime;
    public CharacterController Controller;
    [Property, Group("Animations")] private GameObject AnimatorObject { get; set; }
    [Property, Group("Animations")] private GameObject ShadowAnimatorObject { get; set; }

    private AnimationController Animator;
    private AnimationController[] Animators;


    public Vector3 WishVelocity;
    private bool IsCrouching;
    private Angles Recoil { get; set; }
    private Inventory inventory { get; set; }
    private CameraController camController;

    // private WeaponManager WeaponManager;
    // private TimeSince RecoilTimeSince { get; set; }
    // private const float RecoilDuration = 0.25f;

    // how long after shooting do we continue affecting the next shot from a previous recoil
    // private TimeSince RecoilResetTime { get; set; }
    // private const float RecoilResetCooldown = 0.5f;
    // public Action<bool> OnAimChanged;

    private enum MoveModes
    {
        PLAYER_DIRECTION,
        WORLD_DIRECTION,
    }

    [Property, Group("Move")]
    private MoveModes MoveMode { get; set; }

    [Property, Group("Move")]
    public ViewModes ViewMode { get; set; }

    public Action<ViewModes> OnViewModeChangedEvent;
    private bool hasStarted;

    protected override void OnAwake()
    {
        base.OnAwake();

        Instance = this;
        playerManager = Components.Get<PlayerManager>();
        if (AnimatorObject.IsValid())
        {
            Animator = Components.GetInDescendantsOrSelf<AnimationController>();
        }
        else
        {
            Animator = AnimatorObject.Components.Get<AnimationController>();
        }

        var shadows = ShadowAnimatorObject.Components.Get<AnimationController>();

        Animators = new AnimationController[2];
        Animators[0] = Animator;
        Animators[1] = shadows;

        Controller = Components.GetInDescendantsOrSelf<CharacterController>();
        // WeaponManager = Components.Get<WeaponManager>();
        inventory = Components.Get<Inventory>();

        if (Controller.IsValid())
        {
            Controller.Height = StandHeight;
        }

        camController = Scene.GetAllComponents<CameraController>().FirstOrDefault();

        // ResetViewAngles();
        // OnViewModeChanged();
        Animator.Target.SetBodyGroup("head", 1);
        ResetViewAngles();
    }

    protected override void OnStart()
    {
        base.OnStart();
        hasStarted = true;
    }

    protected override void OnPreRender()
    {
        base.OnPreRender();

        if (Eye.IsValid() && hasStarted)
        {
            if (!Controller.IsValid())
            {
                Controller = Components.Get<CharacterController>();
            }

            var idealEyePos = Eye.Transform.Position;
            var headPosition = Transform.Position + Vector3.Up * Controller.Height;

            var headTrace = Scene.Trace.Ray(Transform.Position, headPosition)
                .UsePhysicsWorld()
                .IgnoreGameObjectHierarchy(GameObject)
                .Run();

            // headPosition = headTrace.EndPosition - headTrace.Direction * 2f;

            var trace = Scene.Trace.Ray(headPosition, idealEyePos)
                .UsePhysicsWorld()
                .IgnoreGameObjectHierarchy(GameObject)
                .WithAnyTags("map", "solid", "player")
                .Radius(20f)
                .Run();

            if (camController.IsValid())
            {
                if (trace.Hit)
                {
                    Scene.Camera.Transform.Position = trace.EndPosition;
                }

                // var eyePos = trace.Hit ? trace.EndPosition : idealEyePos;
                // var eyeRot = EyeAngles.ToRotation() * Rotation.FromPitch(-10f);
                // Scene.Camera.Transform.Position = trace.Hit ? trace.EndPosition : idealEyePos;
                // Scene.Camera.Transform.Rotation = EyeAngles.ToRotation() * Rotation.FromPitch(-10f);
                // camController.SetAngles(eyePos, eyeRot);
            }
            else
            {
                camController = Scene.GetAllComponents<CameraController>().FirstOrDefault();
            }
        }
    }

    protected override void OnFixedUpdate()
    {
        if (playerManager.PlayerState == PlayerManager.PlayerStates.ALIVE)
            HandleMove();
    }

    private void HandleMove()
    {
        BuildWishVelocity();
        // HandleCrouching();

        if (Controller.IsOnGround && Input.Down("Jump"))
        {
            Controller.Punch(Vector3.Up * JumpVelocity);

            foreach (AnimationController anim in Animators)
            {
                anim.TriggerJump();
            }
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
        if (playerManager.PlayerState == PlayerManager.PlayerStates.DEAD) return;

        UpdateLook();
        UpdateAnimator();
        // HandleAiming();

        if (Input.Pressed("Voice"))
        {
            // OnViewModeChanged();
        }
    }

    private void UpdateAnimator()
    {
        // Animator.IsGrounded = Controller.IsOnGround;
        // Animator.MoveStyle = MoveSpeed >= MinRunSpeed ? AnimationController.MoveStyles.Run : AnimationController.MoveStyles.Walk;
        // Animator.DuckLevel = IsCrouching ? 1f : 0f;
        // Animator.FootShuffle = 0f;
        //
        // Animator.WithVelocity(Controller.Velocity);
        // Animator.WithWishVelocity(WishVelocity);
        // Animator.WithLook(EyeAngles.Forward);
        // Animator.UpdateIk();


        for (var i = 0; i < Animators.Length; i++)
        {
            AnimationController anim = Animators[i];
            if (!anim.IsValid())
            {
                Log.Warning($"Animator `{i}` not valid");
                continue;
            }

            anim.WithVelocity(Controller.Velocity);
            anim.WithWishVelocity(WishVelocity);
            anim.IsGrounded = Controller.IsOnGround;
            anim.WithLook(EyeAngles.Forward, 1, 1, 1.0f);
            bool isRunning = MoveSpeed >= MinRunSpeed;
            anim.MoveStyle = isRunning ? AnimationController.MoveStyles.Run : AnimationController.MoveStyles.Walk;
        }
    }

    public void UpdateAnimatorOnDeath()
    {
        foreach (AnimationController anim in Animators)
        {
            anim.IsGrounded = true;
            anim.FootShuffle = 0;
            anim.WithVelocity(Vector3.Zero);
            anim.WithWishVelocity(Vector3.Zero);
        }
    }

    private void UpdateLook()
    {
        var angles = EyeAngles;
        angles += Input.AnalogLook * 0.5f;
        angles.roll = 0;
        EyeAngles = angles;

        // if (RecoilTimeSince < RecoilDuration)
        // {
        // // Do Recoil
        // }

        var lookDir = EyeAngles.ToRotation();

        if (ViewMode == ViewModes.FIRST_PERSON)
        {
            // cam.Transform.Position = Eye.Transform.Position;
            // cam.Transform.Rotation = lookDir;
            if (Eye.IsValid())
                camController.SetAngles(Eye.Transform.Position, lookDir);
        }

        // angles += Recoil * Time.Delta;
        angles.pitch = angles.pitch.Clamp(-60f, 70f);
        EyeAngles = angles.WithRoll(0f);
    }

    // public void ApplyRecoil(Angles recoil)
    // {
    //     if (RecoilResetTime > RecoilResetCooldown)
    //     {
    //         // Recoil = new Angles(0f);
    //     }
    //
    //     // Recoil += recoil;
    //
    //     RecoilTimeSince = 0;
    //     RecoilResetTime = 0;
    // }


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
            // OnAimChanged?.Invoke(IsAiming);
        }
    }

    // private void OnAimChanged()
    // {
    //     if (!inventory.HasItem) return;
    //
    //     var weapon = inventory.ActiveWeapon;
    //     if (!weapon.IsValid()) return;
    //     weapon.OnAimChanged(IsAiming);
    // }
}