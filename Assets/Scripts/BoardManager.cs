using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BoardManager : MonoBehaviour
{
    [Header("Board References")]
    public Transform slidingBoard;     // The actual popup Board GameObject to slide in
    public Transform centerTarget;     // Empty Transform placed in center screen where board sits when open
    public Collider2D exitButton;      // Drag the exit button/X object collider here

    [Header("Animation Settings")]
    public float moveDuration = 0.35f;  // Seconds to move in/out
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f); // Smooth movement

    private Vector3 offscreenPosition;
    private bool isOpen = false;
    private Coroutine moveCoroutine;

    void Start()
    {
        if (slidingBoard != null)
        {
            // Store starting position off-screen
            offscreenPosition = slidingBoard.position;
        }
    }

    void OnMouseDown()
    {
        // Clicking the background board opens the overlay
        if (!isOpen)
        {
            OpenBoard();
        }
    }

    void Update()
    {
        // Check if player clicks the designated exit button while the board is open
        if (isOpen && exitButton != null && Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0f;

            Collider2D hit = Physics2D.OverlapPoint(mouseWorld);

            if (hit == exitButton)
            {
                CloseBoard();
            }
        }
    }

    public void OpenBoard()
    {
        if (isOpen || slidingBoard == null || centerTarget == null) return;

        isOpen = true;
        SfxBank.Play(SfxId.CupPickUp);

        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(AnimateBoard(slidingBoard.position, centerTarget.position));
    }

    public void CloseBoard()
    {
        if (!isOpen || slidingBoard == null) return;

        isOpen = false;
        SfxBank.Play(SfxId.CupPlaced);

        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(AnimateBoard(slidingBoard.position, offscreenPosition));
    }

    IEnumerator AnimateBoard(Vector3 startPos, Vector3 endPos)
    {
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);

            float easedT = easeCurve.Evaluate(t);
            slidingBoard.position = Vector3.LerpUnclamped(startPos, endPos, easedT);

            yield return null;
        }

        slidingBoard.position = endPos;
    }
}