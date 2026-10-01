using System.Collections;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameMode { Normal, Endless }

    [Header("Game Mode")]
    public GameMode currentMode = GameMode.Normal;

    [Header("Shift Settings (Normal Mode)")]
    public float shiftDurationInSeconds = 120f; // Real-world seconds for 21:00 -> 06:00
    
    [Header("Sunrise Color Shift")]
    [Range(0f, 1f)]
    public float sunriseStartProgress = 0.6f;
    public Color nightColor = new Color(0.52f, 0.52f, 0.52f, 1f); // #858585
    public Color sunriseColor = new Color(1f, 0.85f, 0.75f, 1f);
    public Color dayColor = Color.white;

    [Header("UI & Environment References")]
    public TextMeshPro timeTextWorld;       // World-space TMP time clock
    public SpriteRenderer sceneryRenderer;

    [Header("Result Board References")]
    public Transform winBoard;           // WinBoard GameObject
    public Transform loseBoard;          // LoseBoard GameObject
    public Transform resultBoardPosition;// Position where result boards land in camera view
    public float boardMoveDuration = 0.4f;
    public AnimationCurve boardEase = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Result Stats World TMP")]
    public TextMeshPro winStatsText;  // World-space TextMeshPro on Win Board
    public TextMeshPro loseStatsText; // World-space TextMeshPro on Lose Board

    // Order Counters
    public int CorrectOrders { get; private set; } = 0;
    public int WrongOrders { get; private set; } = 0;
    public int TotalOrders => CorrectOrders + WrongOrders;

    // STARTS AS FALSE UNTIL INTRO MODE SELECTION IS CLICKED
    public bool IsGameActive { get; private set; } = false;

    private float elapsedTime;
    private int lastDisplayedMinute = -1;
    private int lastDisplayedHour = -1;

    private readonly int startHour = 21; // 21:00
    private readonly int endHour = 30;   // 06:00 next day

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        IsGameActive = false; // Freeze clock immediately on load
    }

    void Start()
    {
        elapsedTime = 0f;
        CorrectOrders = 0;
        WrongOrders = 0;
        Time.timeScale = 1f;

        if (sceneryRenderer != null)
            sceneryRenderer.color = nightColor;

        if (currentMode == GameMode.Normal)
            UpdateTimeDisplay("21:00");
        else
            UpdateTimeDisplay("00:00");
    }

    // Called by IntroManager when mode button is clicked
    public void StartGameSequence(GameMode mode)
    {
        SetGameMode(mode);
        IsGameActive = true;
        elapsedTime = 0f;
        Debug.Log($"[GameManager] Game Started in {mode} mode!");
    }

    void Update()
    {
        if (!IsGameActive) return;

        elapsedTime += Time.deltaTime;

        if (currentMode == GameMode.Normal)
        {
            UpdateNormalMode();
        }
        else
        {
            UpdateEndlessMode();
        }
    }

    public void RecordCorrectOrder()
    {
        CorrectOrders++;
    }

    public void RecordWrongOrder()
    {
        WrongOrders++;
    }

    void UpdateNormalMode()
    {
        float progress = Mathf.Clamp01(elapsedTime / shiftDurationInSeconds);

        if (sceneryRenderer != null)
        {
            if (progress < sunriseStartProgress)
            {
                sceneryRenderer.color = nightColor;
            }
            else
            {
                float sunriseT = (progress - sunriseStartProgress) / (1f - sunriseStartProgress);
                
                if (sunriseT < 0.5f)
                    sceneryRenderer.color = Color.Lerp(nightColor, sunriseColor, sunriseT * 2f);
                else
                    sceneryRenderer.color = Color.Lerp(sunriseColor, dayColor, (sunriseT - 0.5f) * 2f);
            }
        }

        float currentTotalHours = Mathf.Lerp(startHour, endHour, progress);
        int hours = Mathf.FloorToInt(currentTotalHours);
        int rawMinutes = Mathf.FloorToInt((currentTotalHours - hours) * 60f);

        int discreteMinutes = (rawMinutes / 10) * 10;
        int displayHour = hours % 24;

        if (displayHour != lastDisplayedHour || discreteMinutes != lastDisplayedMinute)
        {
            lastDisplayedHour = displayHour;
            lastDisplayedMinute = discreteMinutes;
            UpdateTimeDisplay($"{displayHour:D2}:{discreteMinutes:D2}");
        }

        if (progress >= 1f)
        {
            TriggerWin();
        }
    }

    void UpdateEndlessMode()
    {
        int totalSeconds = Mathf.FloorToInt(elapsedTime);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        UpdateTimeDisplay($"{minutes:D2}:{seconds:D2}");
    }

    void UpdateTimeDisplay(string timeFormatted)
    {
        if (timeTextWorld != null) 
            timeTextWorld.text = timeFormatted;
    }

    public void SetGameMode(GameMode mode)
    {
        currentMode = mode;
        if (currentMode == GameMode.Normal)
            UpdateTimeDisplay("21:00");
        else
            UpdateTimeDisplay("00:00");
    }

    public void TriggerWin()
    {
        if (!IsGameActive) return;

        IsGameActive = false;
        UpdateTimeDisplay("06:00");

        SfxBank.Play(SfxId.ShiftWinChime);
        UpdateResultStatsText(winStatsText);

        if (winBoard != null && resultBoardPosition != null)
        {
            StartCoroutine(ShowResultBoard(winBoard));
        }
        else
        {
            Time.timeScale = 0f;
        }
    }

    public void TriggerGameOver()
    {
        if (!IsGameActive) return;

        IsGameActive = false;
        SfxBank.Play(SfxId.GameOverNightmare);
        UpdateResultStatsText(loseStatsText);

        if (loseBoard != null && resultBoardPosition != null)
        {
            StartCoroutine(ShowResultBoard(loseBoard));
        }
        else
        {
            Time.timeScale = 0f;
        }
    }

    void UpdateResultStatsText(TextMeshPro statsText)
    {
        if (statsText == null) return;

        statsText.text = $"correct orders: {CorrectOrders}\n" +
                         $"wrong orders: {WrongOrders}\n" +
                         $"total orders: {TotalOrders}";
    }

    IEnumerator ShowResultBoard(Transform board)
    {
        SfxBank.Play(SfxId.BoardWhoosh);

        float elapsed = 0f;
        Vector3 startPos = board.position;
        Vector3 endPos = resultBoardPosition.position;

        while (elapsed < boardMoveDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / boardMoveDuration);
            float easedT = boardEase.Evaluate(t);

            board.position = Vector3.LerpUnclamped(startPos, endPos, easedT);
            yield return null;
        }

        board.position = endPos;
        Time.timeScale = 0f;
    }
}