using UnityEngine;

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
