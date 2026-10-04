using System;

// The single source of truth for the current GameState. Its only jobs are
// to hold the state, change it, and tell other systems that it changed.
// It knows nothing about scenes, levels, Time.timeScale or gameplay -
// GameManager decides *when* the state changes and asks this class to do it.
// It is a plain static class (not a MonoBehaviour) because it needs no
// GameObject, Inspector or Update, and there can never be two copies of it.
public static class GameStateManager
{
    public static GameState CurrentState { get; private set; } = GameState.MainMenu;

    public static event Action<GameState> OnGameStateChanged;

    public static void ChangeState(GameState newState)
    {
        CurrentState = newState;
        OnGameStateChanged?.Invoke(newState);
    }
}
