using Sandbox;

namespace Kira;

[Group("Kira/Player")]
public sealed class Flashlight : Component
{
    [Property]
    public bool LightOn { get; set; }

    private Light light;

    protected override void OnAwake()
    {
        base.OnAwake();
        light = Components.Get<Light>(true);
        LightOn = light.Enabled;
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        if (Input.Pressed("Flashlight"))
        {
            ToggleLight();
        }
    }

    public void ToggleLight()
    {
        LightOn = !LightOn;
        light.Enabled = LightOn;
    }
}