using UnityEngine;

// Button handlers for the Main Menu scene.
public class MainMenuButtons : MonoBehaviour
{
    public void OnStartButtonPressed()
    {
        GameManager.Instance.StartGame();
    }

    public void OnQuitButtonPressed()
    {
        Application.Quit();
    }
}
