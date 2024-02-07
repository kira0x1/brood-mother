using Sandbox;

public sealed class PlayerController : Component
{
    [Property] public float MoveSpeed { get; set; }

    protected override void OnUpdate()
    {
        Transform.Position += Input.AnalogMove * MoveSpeed * Time.Delta;
    }
}