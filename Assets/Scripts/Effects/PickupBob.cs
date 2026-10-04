using UnityEngine;

// Gently bobs a pickup up and down so it stands out on the floor.
public class PickupBob : MonoBehaviour
{
    [SerializeField] private float height = 0.08f;
    [SerializeField] private float speed = 3f;

    private Vector3 startPosition;
    private float phase;

    private void Start()
    {
        startPosition = transform.position;
        phase = Random.value * Mathf.PI * 2f;
    }

    private void Update()
    {
        transform.position = startPosition + Vector3.up * (Mathf.Sin(Time.time * speed + phase) * height);
    }
}
