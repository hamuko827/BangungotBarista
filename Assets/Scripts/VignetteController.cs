using UnityEngine;

public class VignetteController : MonoBehaviour
{
    public static VignetteController Instance { get; private set; }

    [Header("References")]
    public SpriteRenderer vignetteRenderer; // Drag your SpriteRenderer or Image here

    [Header("Warning Settings")]
    public float dangerTimeThreshold = 7f; // Triggers when <= 7 seconds left
    public Color vignetteColor = Color.black;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (vignetteRenderer == null)
            vignetteRenderer = GetComponent<SpriteRenderer>();

        SetAlpha(0f);
    }

    /// <summary>
    /// Updates vignette intensity based on lowest customer time remaining.
    /// </summary>
    public void UpdateVignette(float lowestPatienceTime)
    {
        if (vignetteRenderer == null) return;

        if (lowestPatienceTime <= dangerTimeThreshold && lowestPatienceTime > 0f)
        {
            // Normalize alpha from 0 (at 7s) to ~0.8 (at 0s)
            float t = 1f - (lowestPatienceTime / dangerTimeThreshold);
            float targetAlpha = Mathf.Lerp(0.1f, 0.85f, t);

            // Subtle pulsing effect for urgency
            float pulse = Mathf.Sin(Time.time * 6f) * 0.1f;
            targetAlpha = Mathf.Clamp01(targetAlpha + pulse);

            SetAlpha(targetAlpha);
        }
        else
        {
            SetAlpha(0f);
        }
    }

    public void SetAlpha(float alpha)
    {
        if (vignetteRenderer == null) return;
        Color c = vignetteColor;
        c.a = alpha;
        vignetteRenderer.color = c;
    }
}