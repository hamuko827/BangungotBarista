using System.Collections;
using UnityEngine;

public class IntroManager : MonoBehaviour
{
    public enum GameMode { Normal, Endless }

    [Header("Camera Setup")]
    public Transform mainCamera;        // Drag Main Camera here
    public Transform gameCameraAnchor;  // Position where camera lands for gameplay
    public float cameraMoveDuration = 1.0f;

    [Header("Board 1 Setup")]
    public Transform board1;            // Board 1 GameObject
    public Collider2D downButton1;      // Next button on Board 1

    [Header("Board 2 Setup")]
    public Transform board2;            // Board 2 GameObject
    public Collider2D downButton2;      // Next button on Board 2

    [Header("Board 3 Setup (Mode Select)")]
    public Transform board3;            // Board 3 GameObject
    public Collider2D normalModeButton; // Normal Mode choice collider
    public Collider2D endlessModeButton;// Endless Mode choice collider

    [Header("Board Anchors")]
    public Transform activeBoardPos;    // Main reading position in front of Intro Camera
    public Transform boardParkingPos;   // Off-screen parking position

    [Header("Animation Settings")]
    public float boardMoveDuration = 0.4f;
    public AnimationCurve boardEase = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public AnimationCurve cameraEase = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    public GameMode SelectedMode { get; private set; }

    private int currentStep = 1;
    private bool isTransitioning = false;

    void Start()
    {
        // Position Board 1 at reading area
        if (board1 != null && activeBoardPos != null)
            board1.position = activeBoardPos.position;

        currentStep = 1;
        isTransitioning = false;
    }

    void Update()
    {
        if (isTransitioning || Input.GetMouseButtonDown(0) == false) return;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        Collider2D hit = Physics2D.OverlapPoint(mouseWorld);

        if (hit == null) return;

        // Step 1: Click Down Button 1
        if (currentStep == 1 && hit == downButton1)
        {
            SfxBank.Play(SfxId.ButtonClick);
            StartCoroutine(TransitionBoardSequence(board1, board2));
            currentStep = 2;
        }
        // Step 2: Click Down Button 2
        else if (currentStep == 2 && hit == downButton2)
        {
            SfxBank.Play(SfxId.ButtonClick);
            StartCoroutine(TransitionBoardSequence(board2, board3));
            currentStep = 3;
        }
        // Step 3: Select Game Mode (THIS IS WHERE THE GAME ACTUALLY STARTS)
        else if (currentStep == 3)
        {
            if (hit == normalModeButton)
            {
                SelectModeAndStartGame(GameMode.Normal);
            }
            else if (hit == endlessModeButton)
            {
                SelectModeAndStartGame(GameMode.Endless);
            }
        }
    }

    void SelectModeAndStartGame(GameMode mode)
    {
        SelectedMode = mode;
        SfxBank.Play(SfxId.ButtonClick);

        // Tell GameManager to activate shift and timer!
        if (GameManager.Instance != null)
        {
            GameManager.GameMode targetMode = (mode == GameMode.Normal) ? 
                GameManager.GameMode.Normal : GameManager.GameMode.Endless;
            
            GameManager.Instance.StartGameSequence(targetMode);
        }

        // Slide Board 3 away and transition camera into game view
        StartCoroutine(ParkBoard(board3));
        StartCoroutine(MoveCameraToGame());
    }

    IEnumerator TransitionBoardSequence(Transform currentBoard, Transform nextBoard)
    {
        isTransitioning = true;
        SfxBank.Play(SfxId.BoardWhoosh);

        float elapsed = 0f;
        Vector3 startCurrent = currentBoard.position;
        Vector3 startNext = nextBoard.position;

        while (elapsed < boardMoveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / boardMoveDuration);
            float easedT = boardEase.Evaluate(t);

            currentBoard.position = Vector3.LerpUnclamped(startCurrent, boardParkingPos.position, easedT);
            nextBoard.position = Vector3.LerpUnclamped(startNext, activeBoardPos.position, easedT);

            yield return null;
        }

        currentBoard.position = boardParkingPos.position;
        nextBoard.position = activeBoardPos.position;
        isTransitioning = false;
    }

    IEnumerator ParkBoard(Transform board)
    {
        SfxBank.Play(SfxId.BoardWhoosh);

        float elapsed = 0f;
        Vector3 startPos = board.position;

        while (elapsed < boardMoveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / boardMoveDuration);
            float easedT = boardEase.Evaluate(t);

            board.position = Vector3.LerpUnclamped(startPos, boardParkingPos.position, easedT);
            yield return null;
        }

        board.position = boardParkingPos.position;
    }

    IEnumerator MoveCameraToGame()
    {
        float elapsed = 0f;
        Vector3 startCamPos = mainCamera.position;
        Vector3 targetCamPos = new Vector3(gameCameraAnchor.position.x, gameCameraAnchor.position.y, mainCamera.position.z);

        while (elapsed < cameraMoveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / cameraMoveDuration);
            float easedT = cameraEase.Evaluate(t);

            mainCamera.position = Vector3.LerpUnclamped(startCamPos, targetCamPos, easedT);
            yield return null;
        }

        mainCamera.position = targetCamPos;
    }
}