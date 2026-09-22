using UnityEngine;

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
