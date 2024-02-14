using System.Collections.Generic;
using Sandbox;

namespace Kira;

[Group("Kira")]
[Title("Player Controller")]
public sealed class PlayerController : Component
{
    [Property] public Vector3 Gravity { get; set; } = new(0f, 0f, 800f);
    [Property] public float TurnSpeed { get; set; } = 500f;
    [Property] public float MoveSpeed { get; set; } = 80f;
    [Property] public float JumpVelocity { get; set; } = 300f;
    [Property] public float StandHeight { get; set; } = 64f;
    [Property] public float CrouchHeight { get; set; } = 28f;
    [Property] public float AimSpeed { get; set; } = 2f;

    // if current speed exceeds this then set anim to running
    [Property] public float MinRunSpeed { get; set; } = 150f;
    [Property] public float StretchIkSpeed { get; set; } = 50f;

    private Angles EyeAngles { get; set; }
    private CharacterController Controller;
    private AnimationController Animator;
    private Vector3 WishVelocity;
    private bool IsCrouching;

    private RealTimeSince LastUngroundedTime { get; set; }
    private Transform StartLeftHandIkTransform { get; set; }
    private SkinnedModelRenderer Target;

    [Property]
    private LimbID LimbSelected { get; set; }

    public Dictionary<LimbID, IkLimb> IkLimbs = new();


    public enum MoveState
    {
        NORMAL,
        STRETCH,
        FREEZE
    }

    [Property] public MoveState CurrentMoveState { get; set; } = MoveState.NORMAL;

    protected override void OnAwake()
    {
        base.OnAwake();

        Animator = Components.GetInDescendantsOrSelf<AnimationController>();
        Controller = Components.GetInDescendantsOrSelf<CharacterController>();

        if (Controller.IsValid())
        {
            Controller.Height = StandHeight;
        }
    }

    protected override void OnStart()
    {
        base.OnStart();

        if (Animator.IsValid())
        {
            StartLeftHandIkTransform = Animator.IkLeftHand.Transform.World;
            Target = Animator.Target;
        }

        IkLimbs = new Dictionary<LimbID, IkLimb>();
        IkLimbs.Add(LimbID.LEFT_FOOT, new IkLimb("foot_left", LimbID.LEFT_FOOT, GetAnimatorIkLimb(LimbID.LEFT_FOOT)));
        IkLimbs.Add(LimbID.RIGHT_FOOT, new IkLimb("foot_right", LimbID.RIGHT_FOOT, GetAnimatorIkLimb(LimbID.RIGHT_FOOT)));
        IkLimbs.Add(LimbID.RIGHT_HAND, new IkLimb("hand_right", LimbID.RIGHT_HAND, GetAnimatorIkLimb(LimbID.RIGHT_HAND)));
        IkLimbs.Add(LimbID.LEFT_HAND, new IkLimb("hand_left", LimbID.LEFT_HAND, GetAnimatorIkLimb(LimbID.LEFT_HAND)));
    }

    protected override void OnFixedUpdate()
    {
        switch (CurrentMoveState)
        {
            case MoveState.NORMAL:
                HandleMove();
                break;
            case MoveState.STRETCH:
                HandleStretch();
                break;
            case MoveState.FREEZE:
                FreezeAllIk();
                break;
        }
    }

    private void HandleMoveStateInput()
    {
        if (Input.Pressed("Slot1"))
        {
            CurrentMoveState = MoveState.NORMAL;
        }
        else if (Input.Pressed("Slot2"))
        {
            foreach (var limb in IkLimbs.Values)
            {
                limb.position = limb.limbObject.Transform.LocalPosition;
            }

            CurrentMoveState = MoveState.STRETCH;
        }
        else if (Input.Pressed("Slot3"))
        {
            FreezeAllIk();
            CurrentMoveState = MoveState.FREEZE;
        }
    }

    private void SetIk(string name, Vector3 position, Vector3 rotation)
    {
        Target.Set($"ik.{name}.enabled", true);
        Target.Set($"ik.{name}.position", position);
        Target.Set($"ik.{name}.rotation", rotation);
    }

    private void FreezeAllIk()
    {
        Animator.ClearIk("hand_right");
        Animator.ClearIk("hand_left");
        Animator.ClearIk("foot_left");
        Animator.ClearIk("foot_right");
    }


    private GameObject GetAnimatorIkLimb(LimbID limbId)
    {
        return limbId switch
        {
            LimbID.RIGHT_FOOT => Animator.IkRightFoot,
            LimbID.LEFT_FOOT => Animator.IkLeftFoot,
            LimbID.LEFT_HAND => Animator.IkLeftHand,
            LimbID.RIGHT_HAND => Animator.IkRightHand,
            _ => Animator.IkLeftFoot
        };
    }

    private void HandleStretch()
    {
        if (Input.Pressed("Score"))
        {
            LimbSelected++;
            if ((int)LimbSelected >= 4) LimbSelected = 0;
        }

        Vector3 stretch = Input.AnalogMove * StretchIkSpeed * Time.Delta;

        IkLimb limb = IkLimbs[LimbSelected];
        Transform limbLocal = limb.limbObject.Transform.Local;

        limb.position += limbLocal.Left * stretch.x;
        limb.position += limbLocal.Forward * stretch.y;
        SetIk(limb.name, limb.position, Vector3.Zero);
    }


    private void HandleMove()
    {
        var inputVal = Input.AnalogMove.Normal.WithZ(0f);
        var fwd = Transform.LocalRotation;
        var finalVal = (inputVal * fwd * MoveSpeed).WithZ(0f);
        // WishVelocity = (Input.AnalogMove.Normal * MoveSpeed).WithZ(0f);
        WishVelocity = finalVal;
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
        float turnAxis = Input.AnalogLook.yaw * TurnSpeed * Time.Delta;
        Transform.LocalRotation = Transform.LocalRotation.RotateAroundAxis(Vector3.Up, turnAxis);
    }

    protected override void OnUpdate()
    {
        UpdateAnimator();
        HandleEyes();
    }

    private void UpdateAnimator()
    {
        if (CurrentMoveState == MoveState.FREEZE) return;

        Animator.IsGrounded = Controller.IsOnGround;
        Animator.MoveStyle = MoveSpeed >= MinRunSpeed ? AnimationController.MoveStyles.Run : AnimationController.MoveStyles.Walk;
        Animator.DuckLevel = IsCrouching ? 1f : 0f;
        Animator.FootShuffle = 0f;

        Animator.WithVelocity(Controller.Velocity);
        Animator.WithWishVelocity(WishVelocity);

        if (CurrentMoveState == MoveState.NORMAL)
        {
            Animator.UpdateIk();
        }
    }

    private void HandleEyes()
    {
        var ee = EyeAngles;
        ee += Input.AnalogLook * AimSpeed;
        ee.roll = 0;
        EyeAngles = ee;

        Animator.WithLook(EyeAngles.Forward, 1, 1, 1.0f);
    }
}