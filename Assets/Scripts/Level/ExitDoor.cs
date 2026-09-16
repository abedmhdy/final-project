using UnityEngine;

// Blocks the way to the next level until the WaveSpawner unlocks it.
// On the last level, walking through it wins the game instead of loading
// another scene.
public class ExitDoor : MonoBehaviour
{
    [SerializeField] private SpriteRenderer doorSprite;
    [SerializeField] private Color lockedColor = Color.gray;
    [SerializeField] private Color unlockedColor = Color.green;
    [SerializeField] private bool isFinalLevel;

    private bool isUnlocked;

    private void Start()
    {
        isUnlocked = false;
        if (doorSprite != null) doorSprite.color = lockedColor;
    }

    public void Unlock()
    {
        isUnlocked = true;
        if (doorSprite != null) doorSprite.color = unlockedColor;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isUnlocked || !other.CompareTag("Player")) return;

        if (isFinalLevel)
        {
            GameManager.Instance.TriggerVictory();
        }
        else
        {
            GameManager.Instance.LoadNextLevel();
        }
    }
}
