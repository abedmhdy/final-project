using UnityEngine;

// Lives on the Canvas, which is never itself disabled, so it always stays
// subscribed to GameStateManager's state event. Shows and hides the right
// panel whenever the GameState changes, instead of the panels managing
// their own visibility (which would unsubscribe them from the very event
// that is supposed to bring them back).
public class UIStateController : MonoBehaviour
{
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject victoryPanel;

    private void OnEnable()
    {
        GameStateManager.OnGameStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        GameStateManager.OnGameStateChanged -= HandleStateChanged;
    }

    private void Start()
    {
        HandleStateChanged(GameStateManager.CurrentState);
    }

    private void HandleStateChanged(GameState state)
    {
        if (hudPanel != null) hudPanel.SetActive(state == GameState.Playing || state == GameState.Paused);
        if (pausePanel != null) pausePanel.SetActive(state == GameState.Paused);
        if (gameOverPanel != null) gameOverPanel.SetActive(state == GameState.GameOver);
        if (victoryPanel != null) victoryPanel.SetActive(state == GameState.Victory);
    }
}
