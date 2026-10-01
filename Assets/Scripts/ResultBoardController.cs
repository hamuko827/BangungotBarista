using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultBoardController : MonoBehaviour
{
    [Header("Lose Board Buttons")]
    public Collider2D retryButton;      // Reloads current scene to play again
    public Collider2D loseExitButton;   // Exits application

    [Header("Win Board Buttons")]
    public Collider2D backToStartButton;// Reloads scene to reset to tutorial/mode select
    public Collider2D winExitButton;    // Exits application

    void Update()
    {
        // Only process clicks if a result board is visible / game isn't active
        if (GameManager.Instance != null && GameManager.Instance.IsGameActive) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0f;

            Collider2D hit = Physics2D.OverlapPoint(mouseWorld);
            if (hit == null) return;

            // --- LOSE BOARD ACTIONS ---
            if (hit == retryButton)
            {
                SfxBank.Play(SfxId.ButtonClick);
                RestartGame();
            }
            else if (hit == loseExitButton)
            {
                SfxBank.Play(SfxId.ButtonClick);
                ExitGame();
            }

            // --- WIN BOARD ACTIONS ---
            else if (hit == backToStartButton)
            {
                SfxBank.Play(SfxId.ButtonClick);
                RestartGame();
            }
            else if (hit == winExitButton)
            {
                SfxBank.Play(SfxId.ButtonClick);
                ExitGame();
            }
        }
    }

    void RestartGame()
    {
        // Unfreeze time before reloading scene!
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void ExitGame()
    {
        Debug.Log("Exiting Game...");
        Application.Quit();

#if UNITY_EDITOR
        // Allows testing exit button inside Unity Editor
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}