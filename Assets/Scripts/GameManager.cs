using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public int score = 0;
    public bool isGameOver = false;
    public bool isPaused = false;

    // New flag to block tree clicks during a jump
    public bool isInteracting = false;
    private void Awake()
    {
        Debug.Log("GameManager Awake called");
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("GameManager instance created");
        }
        else
        {
            Debug.Log("GameManager instance already exists, destroying duplicate");
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Ensure game starts unpaused
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void AddScore(int value)
    {
        Debug.Log($"Adding score: {value}. Current score: {score}");
        score += value;
        UIManager.Instance.UpdateScore(score);
    }

    public void GameOver()
    {
        Debug.Log("Game Over called");
        isGameOver = true;
        Time.timeScale = 0f; // Pause the game
        UIManager.Instance.ShowGameOverMenu();
    }

    public void RestartGame()
    {
        Debug.Log("Restarting game");
        ResetGameState();

        // Load the game scene
        LevelManager.Instance?.ResetLevel();
        SceneManager.LoadScene("Game");
        
        // Wait for the scene to load and then setup the level
        StartCoroutine(SetupLevelAfterSceneLoad());
    }

    public void ResetGameState()
    {
        score = 0;
        isGameOver = false;
        isPaused = false;
        Time.timeScale = 1f;
    }

    private IEnumerator SetupLevelAfterSceneLoad()
    {
        // Wait for the scene to load
        yield return new WaitForSeconds(0.1f);
        
        // Find the LevelManager in the new scene using the new recommended method
        LevelManager levelManager = FindFirstObjectByType<LevelManager>();
        if (levelManager != null)
        {
            Debug.Log("Setting up level after scene load");
            levelManager.SetupLevel();
        }
        else
        {
            Debug.LogError("LevelManager not found in the new scene!");
        }
    }

    public void GoToMainMenu()
    {
        Debug.Log("Going to main menu");
        ResetGameState();
        LevelManager.Instance?.ResetLevel();
        SceneManager.LoadScene("MainMenu");
    }

    public void TogglePause()
    {
        Debug.Log($"Toggling pause. Current state: {isPaused}");
        isPaused = !isPaused;
        
        // Set time scale before showing/hiding menu
        Time.timeScale = isPaused ? 0f : 1f;
        Debug.Log($"Time scale set to: {Time.timeScale}");
        
        if (isPaused)
        {
            Debug.Log("Game paused");
            UIManager.Instance.ShowPauseMenu();
        }
        else
        {
            Debug.Log("Game resumed");
            UIManager.Instance.HidePauseMenu();
        }
    }
} 