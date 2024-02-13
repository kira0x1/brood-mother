using Sandbox;

namespace Kira;

[Group("Kira")]
[Title("Vitals")]
[Icon("vital_signs")]
public sealed class CharacterVitals
{
    [Property] public float Health = 100f;
    [Property] public float MaxHealth = 100f;
}