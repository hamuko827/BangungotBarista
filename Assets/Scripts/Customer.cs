using UnityEngine;

public enum SleepState { Awake, SlightlySleepy, ReallySleepy, Taken }

public class Customer : MonoBehaviour
{
    [Header("Data & Configuration")]
    public CustomerData data;

    [Header("Randomized Patience Range (Seconds)")]
    public float minPatience = 40f;
    public float maxPatience = 60f;

    [Header("Available Menu Items")]
    public DrinkRecipe[] availableRecipes;

    [Header("Visual References")]
    public SpriteRenderer customerRenderer;
    public SpriteRenderer bubbleRenderer;
    public Transform orderIconAnchor;

    [Header("Patience Fill Bar")]
    public Transform patienceBarFill; // Drag the fill bar transform here
    public SpriteRenderer patienceBarRenderer; // Optional: for dynamic color changes
    public Color awakeColor = Color.green;
    public Color slightlySleepyColor = Color.yellow;
    public Color reallySleepyColor = Color.red;

    public SleepState CurrentState { get; private set; }
    public DrinkRecipe TargetRecipe { get; private set; }
    public CupType TargetCupType { get; private set; }

    public float MaxWaitTime { get; private set; }
    public float CurrentTimer { get; private set; }
    public float PatienceRatio => Mathf.Clamp01(CurrentTimer / MaxWaitTime);

    private GameObject currentOrderIcon;
    private Vector3 initialBarScale = Vector3.one;

    void Start()
    {
        if (patienceBarFill != null)
        {
            initialBarScale = patienceBarFill.localScale;
        }

        // Apply patience overrides from CustomerData if available
        if (data != null)
        {
            if (data.minPatience > 0f) minPatience = data.minPatience;
            if (data.maxPatience > 0f) maxPatience = data.maxPatience;
        }

        GenerateNewOrder();
    }

    void Update()
    {
        // 1. Freeze patience completely until a mode is selected and game starts
        if (GameManager.Instance == null || !GameManager.Instance.IsGameActive) return;

        if (CurrentState == SleepState.Taken) return;

        CurrentTimer -= Time.deltaTime;
        float ratio = PatienceRatio;

        // Update visual fill bar scale
        if (patienceBarFill != null)
        {
            patienceBarFill.localScale = new Vector3(initialBarScale.x * ratio, initialBarScale.y, initialBarScale.z);
        }

        // Trigger edge vignette overlay when patience hits the final 7 seconds
        if (CurrentTimer <= 7f && VignetteController.Instance != null)
        {
            VignetteController.Instance.UpdateVignette(CurrentTimer);
        }

        // State Machine & Screen Shake Trigger
        if (ratio > 0.66f && CurrentState != SleepState.Awake)
        {
            CurrentState = SleepState.Awake;
            UpdateVisuals();
        }
        else if (ratio <= 0.66f && ratio > 0.33f && CurrentState != SleepState.SlightlySleepy)
        {
            CurrentState = SleepState.SlightlySleepy;
            UpdateVisuals();
        }
        else if (ratio <= 0.33f && ratio > 0f && CurrentState != SleepState.ReallySleepy)
        {
            CurrentState = SleepState.ReallySleepy;
            UpdateVisuals();

            // Screen shake triggers ONLY when entering ReallySleepy
            if (CameraShake.Instance != null)
                CameraShake.Instance.Shake(0.2f, 0.08f);
        }
        else if (ratio <= 0f && CurrentState != SleepState.Taken)
        {
            CurrentState = SleepState.Taken;
            UpdateVisuals();

            // Heavy strike shake on Game Over
            if (CameraShake.Instance != null)
                CameraShake.Instance.Shake(0.5f, 0.25f);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerGameOver();
            }
        }
    }

    public void GenerateNewOrder()
    {
        if (availableRecipes == null || availableRecipes.Length == 0) return;

        MaxWaitTime = Random.Range(minPatience, maxPatience);
        CurrentTimer = MaxWaitTime;

        int recipeIndex = Random.Range(0, availableRecipes.Length);
        TargetRecipe = availableRecipes[recipeIndex];
        TargetCupType = (CupType)Random.Range(0, 2);

        CurrentState = SleepState.Awake;
        SetupOrderDisplay();
        UpdateVisuals();
    }

    void SetupOrderDisplay()
    {
        if (TargetRecipe == null || orderIconAnchor == null) return;

        if (currentOrderIcon != null) Destroy(currentOrderIcon);

        GameObject prefab = TargetRecipe.PrefabFor(TargetCupType);
        if (prefab != null)
        {
            currentOrderIcon = Instantiate(prefab, orderIconAnchor);
            currentOrderIcon.transform.localPosition = Vector3.zero;

            if (currentOrderIcon.TryGetComponent(out Draggable d)) Destroy(d);
            if (currentOrderIcon.TryGetComponent(out Collider2D c)) Destroy(c);
        }
    }

    void UpdateVisuals()
    {
        if (data == null) return;

        switch (CurrentState)
        {
            case SleepState.Awake:
                customerRenderer.sprite = data.awakeSprite;
                bubbleRenderer.sprite = data.awakeBubbleSprite;
                if (patienceBarRenderer != null) patienceBarRenderer.color = awakeColor;
                break;
            case SleepState.SlightlySleepy:
                customerRenderer.sprite = data.slightlySleepySprite;
                bubbleRenderer.sprite = data.slightlySleepyBubbleSprite;
                if (patienceBarRenderer != null) patienceBarRenderer.color = slightlySleepyColor;
                break;
            case SleepState.ReallySleepy:
                customerRenderer.sprite = data.reallySleepySprite;
                bubbleRenderer.sprite = data.reallySleepyBubbleSprite;
                if (patienceBarRenderer != null) patienceBarRenderer.color = reallySleepyColor;
                break;
        }
    }

    public bool ReceiveDrink(Drink drink)
    {
        if (CurrentState == SleepState.Taken) return false;

        if (drink.Recipe == TargetRecipe && drink.Type == TargetCupType)
        {
            SfxBank.Play(SfxId.DrinkFinished);

            // Clear active vignette warning when successfully served
            if (VignetteController.Instance != null)
            {
                VignetteController.Instance.SetAlpha(0f);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.RecordCorrectOrder();
            }

            GenerateNewOrder();
            return true;
        }
        else
        {
            float penalty = MaxWaitTime * 0.20f;
            CurrentTimer = Mathf.Max(0f, CurrentTimer - penalty);
            SfxBank.Play(SfxId.BrewFailed);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.RecordWrongOrder();
            }

            return false;
        }
    }
}