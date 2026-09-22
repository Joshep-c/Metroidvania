using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Paneles de UI")]
    public GameObject gameOverPanel;
    public GameObject winPanel;

    private int enemyCount = 0;
    private bool gameEnded = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    // Cada enemigo llama esto en su Awake, para que el GameManager sepa cuántos hay en total.
    public void RegisterEnemy()
    {
        enemyCount++;
    }

    // Cada enemigo llama esto justo antes de destruirse.
    public void EnemyDefeated()
    {
        if (gameEnded) return;

        enemyCount--;
        if (enemyCount <= 0)
        {
            ShowWin();
        }
    }

    // El jugador llama esto cuando su vida llega a 0.
    public void PlayerDied()
    {
        if (gameEnded) return;
        ShowGameOver();
    }

    private void ShowGameOver()
    {
        gameEnded = true;
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    private void ShowWin()
    {
        gameEnded = true;
        if (winPanel != null) winPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}