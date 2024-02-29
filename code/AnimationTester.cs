using System;

namespace Kira;

[Group("Kira")]
[Title("Animation Tester")]
public sealed class AnimationTester : Component
{
    private AnimationController Animator { get; set; }

    protected override void OnStart()
    {
        base.OnStart();
        Animator = Components.Get<AnimationController>();
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        if (Input.Released("Slot1"))
        {
            Animator.FaceOverride = AnimationController.FaceOverrides.None;
        }
        else if (Input.Released("Slot2"))
        {
            Animator.FaceOverride = AnimationController.FaceOverrides.Smile;
        }
        else if (Input.Released("Slot3"))
        {
            Animator.FaceOverride = AnimationController.FaceOverrides.Frown;
        }

        if (Input.Pressed("Voice"))
        {
            int faceMode = Random.Shared.Int(0, 5);
            Animator.FaceOverride = (AnimationController.FaceOverrides)faceMode;
        }
    }
}