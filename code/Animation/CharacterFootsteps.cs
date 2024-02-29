namespace Kira;

// Found this in the FacePunch.Arena project

[Group("Kira")]
[Title("Character Footsteps")]
public sealed class CharacterFootsteps : Component
{
    [Property, Range(0, 5f)] private float VolumeBoost { get; set; } = 1.0f;
    [Property] private SkinnedModelRenderer ModelRenderer { get; set; }
    private TimeSince TimeSinceLastStep { get; set; }

    protected override void OnEnabled()
    {
        if (!ModelRenderer.IsValid()) return;

        ModelRenderer.OnFootstepEvent += OnFootstep;
    }

    protected override void OnDisabled()
    {
        if (!ModelRenderer.IsValid()) return;

        ModelRenderer.OnFootstepEvent -= OnFootstep;
    }

    private void OnFootstep(SceneModel.FootstepEvent e)
    {
        if (TimeSinceLastStep < 0.2f) return;

        SceneTraceResult trace = Scene.Trace.Ray(e.Transform.Position + Vector3.Up * 20f, e.Transform.Position + Vector3.Up * -20f).Run();
        if (!trace.Hit) return;

        if (trace.Surface is null) return;

        TimeSinceLastStep = 0f;

        var sound = e.FootId == 0 ? trace.Surface.Sounds.FootLeft : trace.Surface.Sounds.FootRight;
        if (sound is null) return;

        var handle = Sound.Play(sound, trace.HitPosition + trace.Normal * 10f);
        handle.Volume *= VolumeBoost + e.Volume;
        handle.Update();
    }
}