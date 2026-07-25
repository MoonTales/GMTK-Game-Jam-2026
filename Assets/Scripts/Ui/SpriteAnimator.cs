using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class SpriteAnimator : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite[] frames;
    [SerializeField] private Sprite disabledSprite;

    [Header("Settings")]
    [SerializeField] private float framesPerSecond = 10f;
    public bool isDisabled = false;
    public bool playBackwards = false;

    private Image imageComponent;
    private int currentFrame;
    private float timer;

    private void Awake()
    {
        imageComponent = GetComponent<Image>();
        SetPlayBackwards(playBackwards);
    }

    private void Update()
    {
        // 1. If disabled, show disabled sprite and stop animating
        if (isDisabled)
        {
            if (disabledSprite != null && imageComponent.sprite != disabledSprite)
            {
                imageComponent.sprite = disabledSprite;
            }
            return;
        }

        // Return early if no frames were provided
        if (frames == null || frames.Length == 0) return;

        // 2. Animate looping frames
        timer += Time.deltaTime;
        float frameDuration = 1f / Mathf.Max(0.001f, framesPerSecond);

        if (timer >= frameDuration)
        {
            timer -= frameDuration;

            if (playBackwards)
            {
                currentFrame--;
                if (currentFrame < 0)
                {
                    currentFrame = frames.Length - 1;
                }
            }
            else
            {
                currentFrame = (currentFrame + 1) % frames.Length;
            }

            imageComponent.sprite = frames[currentFrame];
        }
    }

    /// <summary>
    /// Helper method to toggle active state programmatically or from UI UnityEvents.
    /// </summary>
    public void SetDisabled(bool disabled)
    {
        isDisabled = disabled;
        if (!isDisabled)
        {
            // Reset to the appropriate starting frame based on playback direction
            timer = 0f;
            currentFrame = playBackwards ? frames.Length - 1 : 0;

            if (frames != null && frames.Length > 0)
            {
                imageComponent.sprite = frames[currentFrame];
            }
        }
    }

    /// <summary>
    /// Helper method to flip playback direction at runtime.
    /// </summary>
    public void SetPlayBackwards(bool backwards)
    {
        playBackwards = backwards;
    }
}