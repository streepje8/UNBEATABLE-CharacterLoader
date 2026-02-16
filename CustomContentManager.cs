using Rhythm;
using UnityEngine;

namespace Streep.UNBEATABLE.CharacterLoader;

public class CustomContentManager : MonoBehaviour
{
    public GameObject CustomContent { get; private set; }
    public RhythmPlayerAnimator Animator { get; private set; }
    public bool Ready { get; private set; } = false;
    public void Initialize(GameObject customContent, RhythmPlayerAnimator animator)
    {
        CustomContent = customContent;
        Animator = animator;
        Ready = true;
    }

    private bool lastFlipX = false;
    private void Update()
    {
        if (Ready)
        {
            var newFlipX = Animator.SpriteFlipX;
            if (newFlipX != lastFlipX)
            {
                CustomContent.transform.localScale = new Vector3(newFlipX ? -1 : 1, 1, 1);
                lastFlipX = newFlipX;
            }           
        }
    }
}