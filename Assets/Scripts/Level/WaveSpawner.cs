using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// One enemy to spawn at one point, using its own EnemyData-driven prefab.
[Serializable]
public class EnemySpawnInfo
{
    public GameObject enemyPrefab;
    public Transform spawnPoint;
}

// A group of enemies that spawns together and must be fully defeated
// before the next wave starts.
[Serializable]
public class Wave
{
    public string waveName = "Wave";
    public EnemySpawnInfo[] enemiesToSpawn;
    public float delayBeforeWave = 1f;
}

// Runs the waves for a level in order. Waits for every enemy in a wave to
// die before starting the next one, then unlocks the exit door and lets
// the rest of the level know the room is clear.
public class WaveSpawner : MonoBehaviour
{
    [SerializeField] private Wave[] waves;
    [SerializeField] private ExitDoor exitDoor;

    private readonly List<EnemyHealth> aliveEnemies = new List<EnemyHealth>();

    public static event Action<int, int> OnWaveChanged;
    public static event Action OnAllWavesCleared;

    private void Start()
    {
        StartCoroutine(RunWaves());
    }

    private IEnumerator RunWaves()
    {
        for (int waveIndex = 0; waveIndex < waves.Length; waveIndex++)
        {
            Wave wave = waves[waveIndex];
            yield return new WaitForSeconds(wave.delayBeforeWave);

            OnWaveChanged?.Invoke(waveIndex + 1, waves.Length);
            SpawnWave(wave);

            yield return new WaitUntil(() => aliveEnemies.Count == 0);
        }

        if (exitDoor != null)
        {
            exitDoor.Unlock();
        }

        OnAllWavesCleared?.Invoke();
    }

    private void SpawnWave(Wave wave)
    {
        foreach (EnemySpawnInfo spawnInfo in wave.enemiesToSpawn)
        {
            GameObject enemyObject = Instantiate(spawnInfo.enemyPrefab, spawnInfo.spawnPoint.position, Quaternion.identity);
            EnemyHealth enemyHealth = enemyObject.GetComponent<EnemyHealth>();
            aliveEnemies.Add(enemyHealth);
            enemyHealth.OnDied += HandleEnemyDied;
        }
    }

    private void HandleEnemyDied(EnemyHealth enemy)
    {
        aliveEnemies.Remove(enemy);
    }
}
