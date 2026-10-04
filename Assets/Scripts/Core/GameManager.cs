using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Runs the game flow: starting the game, loading levels, restarting,
// pausing (Time.timeScale), victory, game over and returning to the menu.
// It does not store the GameState itself - whenever the flow needs a new
// state it asks GameStateManager, which notifies the rest of the game.
// Persists across scene loads so the current level survives from the main
// menu through all levels.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private string[] levelSceneNames = { "Level1", "Level2", "Level3" };
    [SerializeField] private string mainMenuSceneName = "MainMenu";

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
        GameState state = GameStateManager.CurrentState;
        if (state == GameState.Playing) PauseGame();
        else if (state == GameState.Paused) ResumeGame();
    }

    public void StartGame()
    {
        currentLevelIndex = 0;
        Time.timeScale = 1f;
        GameStateManager.ChangeState(GameState.Playing);
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
        GameStateManager.ChangeState(GameState.Playing);
        SceneManager.LoadScene(levelSceneNames[currentLevelIndex]);
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        GameStateManager.ChangeState(GameState.Playing);
        SceneManager.LoadScene(levelSceneNames[Mathf.Max(currentLevelIndex, 0)]);
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        GameStateManager.ChangeState(GameState.Paused);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        GameStateManager.ChangeState(GameState.Playing);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        currentLevelIndex = -1;
        GameStateManager.ChangeState(GameState.MainMenu);
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void TriggerVictory()
    {
        Time.timeScale = 0f;
        GameStateManager.ChangeState(GameState.Victory);
    }

    private void HandlePlayerDied()
    {
        Time.timeScale = 0f;
        GameStateManager.ChangeState(GameState.GameOver);
    }
}
