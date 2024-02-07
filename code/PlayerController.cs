using Sandbox;

public sealed class PlayerController : Component
{
    [Property] public float TurnSpeed { get; set; } = 100f;
    [Property] public float MoveSpeed { get; set; } = 50f;

    protected override void OnUpdate()
    {
        Transform.Position += Input.AnalogMove * MoveSpeed * Time.Delta;
        Transform.Rotation += Input.AnalogLook.ToRotation() * TurnSpeed;
    }
}