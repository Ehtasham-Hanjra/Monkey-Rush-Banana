using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public GameObject pauseMenu;
    public GameObject gameOverMenu;
    public Button pauseButton;
    public Button resumeButton;
    public Button mainMenuButton;
    public Button playAgainButton;

    private CanvasGroup pauseMenuCanvasGroup;
    private CanvasGroup gameOverMenuCanvasGroup;
    private EventSystem eventSystem;

    [Header("Game-Over Menu Buttons")]
    public Button gameOverHomeButton;

    private void Awake()
    {
        // Standard singleton guard
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void CacheCanvasGroups()
    {
        // Pause menu
        if (pauseMenu != null)
        {
            pauseMenuCanvasGroup = pauseMenu.GetComponent<CanvasGroup>();
            if (pauseMenuCanvasGroup == null)
                pauseMenuCanvasGroup = pauseMenu.AddComponent<CanvasGroup>();
        }

        // Game-Over menu
        if (gameOverMenu != null)
        {
            gameOverMenuCanvasGroup = gameOverMenu.GetComponent<CanvasGroup>();
            if (gameOverMenuCanvasGroup == null)
                gameOverMenuCanvasGroup = gameOverMenu.AddComponent<CanvasGroup>();
        }
    }
    // ---- hide/show all the world objects when UI is up ----
    private void ToggleGameplayObjects(bool visible)
    {
        // 1) trees
        if (LevelManager.Instance != null && LevelManager.Instance.treesParent != null)
            LevelManager.Instance.treesParent.gameObject.SetActive(visible);

        // 2) monkey
        if (MonkeyController.Instance != null)
            MonkeyController.Instance.gameObject.SetActive(visible);
    }

    private void Start()
    {
        // 1) Grab every UI element in the freshly loaded scene…
        FindUIElements();

        // 2) Cache the CanvasGroups so Hide/Show will work
        CacheCanvasGroups();

        // 3) Attach all your button listeners in one place
        SetupButtonListeners();

        // 4) Hide both menus until you need them
        HidePauseMenu();
        HideGameOverMenu();

        Debug.Log("UIManager fully wired and ready");
    }

    //private void Start()
    //{
    //    Debug.Log("UIManager Start called");

    //    // Only initialize if this is the active instance
    //    if (Instance == this)
    //    {
    //        InitializeUI();
    //    }
    //}
    //private void Start()
    //{
    //    FindUIElements();
    //    SetupButtonListeners();
    //    HidePauseMenu();
    //    HideGameOverMenu();
    //}


    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Game")
        {
            // Re-find UI elements and re-bind button listeners
            FindUIElements();
            SetupButtonListeners();

            // Grab the LevelManager singleton
            LevelManager levelMgr = LevelManager.Instance;
            if (levelMgr == null)
            {
                Debug.LogError("LevelManager instance is null after scene load!");
                return;
            }

            // Find the parent object under which trees should be instantiated
            GameObject treesParentGO = GameObject.Find("TreesParent");
            if (treesParentGO != null)
            {
                levelMgr.treesParent = treesParentGO.transform;
            }
            else
            {
                Debug.LogError("Could not find GameObject named 'TreesParent' in the scene.");
            }

            // Ensure the treePrefab reference is set (assign in Inspector or load from Resources)
            if (levelMgr.treePrefab == null)
            {
                GameObject loadedPrefab = Resources.Load<GameObject>("TreePrefab");
                if (loadedPrefab != null)
                {
                    levelMgr.treePrefab = loadedPrefab;
                    Debug.Log("Loaded TreePrefab from Resources.");
                }
                else
                {
                    Debug.LogError("LevelManager.treePrefab is null and Resources/TreePrefab could not be loaded!");
                }
            }

            // Finally, build the level
            levelMgr.SetupLevel();
        }
    }

    private void EnsureEventSystem()
    {
        eventSystem = FindFirstObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            Debug.Log("Creating new EventSystem");
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystem = eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();
        }
    }

    private void InitializeUI()
    {
        Debug.Log("Initializing UI elements");

        // Find UI elements in the scene
        FindUIElements();

        // Initialize pause menu
        if (pauseMenu != null)
        {
            pauseMenuCanvasGroup = pauseMenu.GetComponent<CanvasGroup>();
            if (pauseMenuCanvasGroup == null)
            {
                pauseMenuCanvasGroup = pauseMenu.AddComponent<CanvasGroup>();
            }
            HidePauseMenu();
            Debug.Log("Pause menu initialized and hidden");
        }
        else
        {
            Debug.LogError("Pause menu reference missing!");
        }

        // Initialize game over menu
        if (gameOverMenu != null)
        {
            gameOverMenuCanvasGroup = gameOverMenu.GetComponent<CanvasGroup>();
            if (gameOverMenuCanvasGroup == null)
            {
                gameOverMenuCanvasGroup = gameOverMenu.AddComponent<CanvasGroup>();
            }
            HideGameOverMenu();
            Debug.Log("Game over menu initialized and hidden");
        }
        else
        {
            Debug.LogError("Game over menu reference missing!");
        }

        // Reset score display
        if (scoreText != null)
        {
            scoreText.text = "Score: 0";
            Debug.Log("Score text reset to 0");
        }
        else
        {
            Debug.LogError("Score text reference missing!");
        }
    }

    private void FindUIElements()
    {
        // Find Canvas in the scene
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas found in the scene!");
            return;
        }

        // Find UI elements within the Canvas
        scoreText = canvas.GetComponentInChildren<TextMeshProUGUI>();
        pauseMenu = canvas.transform.Find("PauseMenu")?.gameObject;
        gameOverMenu = canvas.transform.Find("GameOverMenu")?.gameObject;
        pauseButton = canvas.transform.Find("PauseButton")?.GetComponent<Button>();
        resumeButton = pauseMenu?.transform.Find("ResumeButton")?.GetComponent<Button>();
        mainMenuButton = pauseMenu?.transform.Find("MainMenuButton")?.GetComponent<Button>();
        playAgainButton = gameOverMenu?.transform.Find("PlayAgainButton")?.GetComponent<Button>();
        gameOverHomeButton = gameOverMenu?.transform.Find("HomeButton")?.GetComponent<Button>();

        Debug.Log("UI elements found in scene");
    }

    private void SetupButtonListeners()
    {
        Debug.Log("Setting up button listeners");

        // Clear all existing listeners first
        if (pauseButton != null) pauseButton.onClick.RemoveAllListeners();
        if (resumeButton != null) resumeButton.onClick.RemoveAllListeners();
        if (mainMenuButton != null) mainMenuButton.onClick.RemoveAllListeners();
        if (playAgainButton != null) playAgainButton.onClick.RemoveAllListeners();
        if (gameOverHomeButton != null) gameOverHomeButton.onClick.RemoveAllListeners();

        // Setup pause button
        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(() => {
                Debug.Log("Pause button clicked");
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.TogglePause();
                }
                else
                {
                    Debug.LogError("GameManager instance is null!");
                }
            });
            Debug.Log("Pause button listener set up");
        }
        else
        {
            Debug.LogError("Pause button reference missing!");
        }

        // Setup resume button
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(() => {
                Debug.Log("Resume button clicked");
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.TogglePause();
                }
                else
                {
                    Debug.LogError("GameManager instance is null!");
                }
            });
            Debug.Log("Resume button listener set up");
        }
        else
        {
            Debug.LogError("Resume button reference missing!");
        }

        // Setup main menu button
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(() => {
                Debug.Log("Main menu button clicked");
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.GoToMainMenu();
                }
                else
                {
                    Debug.LogError("GameManager instance is null!");
                }
            });
            Debug.Log("Main menu button listener set up");
        }
        else
        {
            Debug.LogError("Main menu button reference missing!");
        }

        // Setup play again button
        if (playAgainButton != null)
        {
            playAgainButton.onClick.AddListener(() => {
                Debug.Log("Play Again button clicked");
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.RestartGame();
                }
                else
                {
                    Debug.LogError("GameManager instance is null!");
                }
            });
            Debug.Log("Play Again button listener set up");
        }
        else
        {
            Debug.LogError("Play Again button reference missing!");
        }
        if (gameOverHomeButton != null)
                   {
            gameOverHomeButton.onClick.AddListener(() => GameManager.Instance.GoToMainMenu());
                   }
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
        else
        {
            Debug.LogError("Score text reference missing!");
        }
    }

    public void ShowPauseMenu()
    {
        if (pauseMenu != null && pauseMenuCanvasGroup != null)
        {
            ToggleGameplayObjects(false);
            pauseMenu.SetActive(true);
            pauseMenuCanvasGroup.alpha = 1f;
            pauseMenuCanvasGroup.interactable = true;
            pauseMenuCanvasGroup.blocksRaycasts = true;
            Debug.Log("Pause menu is now active");
        }
        else
        {
            Debug.LogError("Pause menu or CanvasGroup reference missing!");
        }
    }

    public void HidePauseMenu()
    {
        if (pauseMenu != null && pauseMenuCanvasGroup != null)
        {
            pauseMenuCanvasGroup.alpha = 0f;
            pauseMenuCanvasGroup.interactable = false;
            pauseMenuCanvasGroup.blocksRaycasts = false;
            pauseMenu.SetActive(false);
            Debug.Log("Pause menu is now hidden");
            ToggleGameplayObjects(true);
        }
        else
        {
            Debug.LogError("Pause menu or CanvasGroup reference missing!");
        }
    }

    public void ShowGameOverMenu()
    {
        Debug.Log("Showing Game Over menu");
        ToggleGameplayObjects(false);
        // Ensure EventSystem exists so buttons can be clicked
        EnsureEventSystem();

        // Re-initialize UI elements and re-find buttons in the current scene
        //InitializeUI();
        // (Re-)find and cache the Play Again and Main Menu buttons
        playAgainButton = gameOverMenu.transform.Find("PlayAgainButton")?.GetComponent<Button>();
        mainMenuButton = gameOverMenu.transform.Find("MainMenuButton")?.GetComponent<Button>();

        // Clear any old listeners, then bind fresh callbacks
        if (playAgainButton != null)
        {
            playAgainButton.onClick.RemoveAllListeners();
            playAgainButton.onClick.AddListener(() => {
                Debug.Log("Play Again button clicked");
                GameManager.Instance.RestartGame();
            });
        }
        else
        {
            Debug.LogError("PlayAgainButton reference missing!");
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(() => {
                Debug.Log("Main Menu button clicked");
                GameManager.Instance.GoToMainMenu();
            });
        }
        else
        {
            Debug.LogError("MainMenuButton reference missing!");
        }

        // Activate and show the Game Over menu
        gameOverMenu.SetActive(true);
        gameOverMenuCanvasGroup.alpha = 1f;
        gameOverMenuCanvasGroup.interactable = true;
        gameOverMenuCanvasGroup.blocksRaycasts = true;

        // Hide all tree hiddenObjects so the field looks �clean�
        foreach (TreeObject tree in Object.FindObjectsByType<TreeObject>(FindObjectsSortMode.None))
        {
            if (tree.hiddenObject != null)
                tree.hiddenObject.SetActive(false);
        }

        Debug.Log("Game Over menu is now active");
    }


    public void HideGameOverMenu()
    {
        if (gameOverMenu != null && gameOverMenuCanvasGroup != null)
        {
            gameOverMenuCanvasGroup.alpha = 0f;
            gameOverMenuCanvasGroup.interactable = false;
            gameOverMenuCanvasGroup.blocksRaycasts = false;
            gameOverMenu.SetActive(false);
            Debug.Log("Game over menu is now hidden");
        }
        else
        {
            Debug.LogError("Game over menu or CanvasGroup reference missing!");
        }
        ToggleGameplayObjects(true);
    }
} 