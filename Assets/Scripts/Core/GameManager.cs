using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Tracks the current GameState and is the only class allowed to change it.
// Persists across scene loads so the state survives from the main menu
// through all levels. Other systems (UI, player) react to state changes
// through the OnGameStateChanged event instead of being called directly.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private string[] levelSceneNames = { "Level1", "Level2", "Level3" };
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    public GameState CurrentState { get; private set; } = GameState.MainMenu;
    public static event Action<GameState> OnGameStateChanged;

    private InputAction pauseAction;
    private int currentLevelIndex = -1;

    private void Awake()
    {
        // Every scene has its own GameManager so the game still works if you
        // press Play from inside a level. Only the first one survives.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        pauseAction = inputActions.FindActionMap("Gameplay").FindAction("Pause");
    }

    private void OnEnable()
    {
        // A duplicate GameManager returns early in Awake without becoming
        // Instance, so it has no pauseAction to wire up - skip it.
        if (Instance != this) return;

        pauseAction.Enable();
        pauseAction.performed += OnPausePerformed;
        PlayerHealth.OnPlayerDied += HandlePlayerDied;
    }

    private void OnDisable()
    {
        if (Instance != this) return;

        pauseAction.performed -= OnPausePerformed;
        pauseAction.Disable();
        PlayerHealth.OnPlayerDied -= HandlePlayerDied;
    }

    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        if (CurrentState == GameState.Playing) PauseGame();
        else if (CurrentState == GameState.Paused) ResumeGame();
    }

    public void StartGame()
    {
        currentLevelIndex = 0;
        Time.timeScale = 1f;
        SetState(GameState.Playing);
        SceneManager.LoadScene(levelSceneNames[currentLevelIndex]);
    }

    public void LoadNextLevel()
    {
        currentLevelIndex++;
        if (currentLevelIndex >= levelSceneNames.Length)
        {
            TriggerVictory();
            return;
        }

        Time.timeScale = 1f;
        SetState(GameState.Playing);
        SceneManager.LoadScene(levelSceneNames[currentLevelIndex]);
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SetState(GameState.Playing);
        SceneManager.LoadScene(levelSceneNames[Mathf.Max(currentLevelIndex, 0)]);
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        SetState(GameState.Paused);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        SetState(GameState.Playing);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        currentLevelIndex = -1;
        SetState(GameState.MainMenu);
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void TriggerVictory()
    {
        Time.timeScale = 0f;
        SetState(GameState.Victory);
    }

    private void HandlePlayerDied()
    {
        Time.timeScale = 0f;
        SetState(GameState.GameOver);
    }

    private void SetState(GameState newState)
    {
        CurrentState = newState;
        OnGameStateChanged?.Invoke(newState);
    }
}
