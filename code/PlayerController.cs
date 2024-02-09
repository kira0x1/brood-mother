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

    private CharacterController Controller;
    private CitizenAnimationHelper Animator;
    private Vector3 WishVelocity;
    private bool IsCrouching;

    private RealTimeSince LastGroundedTime { get; set; }
    private RealTimeSince LastUngroundedTime { get; set; }


    protected override void OnAwake()
    {
        base.OnAwake();

        Animator = Components.GetInDescendantsOrSelf<CitizenAnimationHelper>();
        Controller = Components.GetInDescendantsOrSelf<CharacterController>();

        if (Controller.IsValid())
        {
            Controller.Height = StandHeight;
        }
    }

    protected override void OnFixedUpdate()
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
            LastGroundedTime = 0f;
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
        Animator.DuckLevel = IsCrouching ? 1f : 0f;
        Animator.FootShuffle = 0f;
        Animator.WithVelocity(Controller.Velocity);
        Animator.WithWishVelocity(WishVelocity);
    }
}