using UnityEngine;

// Button handlers shared by the pause, game over and victory panels.
public class InGameMenuButtons : MonoBehaviour
{
    public void OnResumeButtonPressed()
    {
        GameManager.Instance.ResumeGame();
    }

    public void OnRestartButtonPressed()
    {
        GameManager.Instance.RestartLevel();
    }

    public void OnMainMenuButtonPressed()
    {
        GameManager.Instance.GoToMainMenu();
    }
}
