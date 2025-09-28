using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ThrusterAnimator : MonoBehaviour
{
    [Header("Animation Assets")]
    [Tooltip("The array of sprites for the normal thrust animation.")]
    [SerializeField] private Sprite[] thrustFrames;
    [Tooltip("The array of sprites for the boost animation.")]
    [SerializeField] private Sprite[] boostFrames;
    [Tooltip("How many frames to show per second.")]
    [SerializeField] private float animationFramerate = 12f;

    private SpriteRenderer spriteRenderer;
    private int currentFrame;
    private Sprite[] activeAnimation;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // This is a public method the PlayerController will call.
    public void SetState(int state)
    {
        switch (state)
        {
            case 1: // Thrust
                spriteRenderer.enabled = true;
                activeAnimation = thrustFrames;
                break;
            case 2: // Boost
                spriteRenderer.enabled = true;
                activeAnimation = boostFrames;
                break;
            default: // State 0, Idle
                spriteRenderer.enabled = false;
                activeAnimation = null;
                break;
        }
    }

    // We use Update to cycle through the animation frames.
    private void Update()
    {
        // If the renderer is off or there's no active animation, do nothing.
        if (!spriteRenderer.enabled || activeAnimation == null || activeAnimation.Length == 0)
        {
            return;
        }

        // Calculate the current frame based on time and the framerate.
        // The '%' (modulo) operator makes the animation loop.
        int frame = (int)(Time.time * animationFramerate) % activeAnimation.Length;

        // Update the sprite.
        spriteRenderer.sprite = activeAnimation[frame];
    }
}