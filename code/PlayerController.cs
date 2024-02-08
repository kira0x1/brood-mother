using Sandbox;
using Sandbox.Citizen;

public sealed class PlayerController : Component
{
    [Property] public float TurnSpeed { get; set; } = 500f;
    [Property] public float MoveSpeed { get; set; } = 80f;

    private CharacterController controller;
    private CitizenAnimationHelper animHelper;

    protected override void OnStart()
    {
        base.OnStart();
        animHelper = GameObject.Components.Get<CitizenAnimationHelper>();
        controller = GameObject.Components.Get<CharacterController>();
    }

    protected override void OnUpdate()
    {
        float turnAxis = Input.AnalogLook.yaw * TurnSpeed * Time.Delta;
        Transform.LocalRotation = Transform.LocalRotation.RotateAroundAxis(Vector3.Up, turnAxis);

        var velocity = Input.AnalogMove.Normal * MoveSpeed;
        controller.Velocity = velocity;
        controller.Move();

        UpdateAnim(velocity);
    }

    private void UpdateAnim(Vector3 velocity)
    {
        animHelper.WithVelocity(velocity);
    }
}