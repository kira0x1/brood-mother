using Sandbox;
using Sandbox.Citizen;

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

    // if current speed exceeds this then set anim to running
    [Property]
    public float MinRunSpeed { get; set; } = 150f;

    private CharacterController Controller;
    private AnimationController Animator;
    private Vector3 WishVelocity;
    private bool IsCrouching;

    private RealTimeSince LastUngroundedTime { get; set; }

    private Transform StartLeftHandIkTransform { get; set; }
    private Vector3 CurLeftHandIkPos { get; set; }

    public enum MoveState
    {
        NORMAL,
        STUNNED,
        STRETCH
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

        if (Animator.IsValid())
        {
        }
    }

    protected override void OnStart()
    {
        base.OnStart();

        StartLeftHandIkTransform = Animator.IkLeftHand.Transform.World;
        Log.Info($"Start IK: {StartLeftHandIkTransform}");
    }

    protected override void OnFixedUpdate()
    {
        if (Input.Pressed("Slot1"))
        {
            CurrentMoveState = MoveState.NORMAL;
        }
        else if (Input.Pressed("Slot2"))
        {
            // CurLeftHandIkPos = StartLeftHandIkPos.Position;
            CurrentMoveState = MoveState.STRETCH;
        }

        switch (CurrentMoveState)
        {
            case MoveState.NORMAL:
                HandleMove();
                break;
            case MoveState.STRETCH:
                HandleStretch();
                break;
        }
    }

    private void HandleStretch()
    {
        var Target = Animator.Target;
        Target.Set($"ik.left_hand.enabled", true);
        Target.Set($"ik.left_hand.position", new Vector3(1, 0, 0));
        Target.Set($"ik.left_hand.rotation", new Vector3(1, 0, 0));
    }

    private void HandleMove()
    {
        WishVelocity = (Input.AnalogMove.Normal * MoveSpeed).WithZ(0f);
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
    }

    private void UpdateAnimator()
    {
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
}