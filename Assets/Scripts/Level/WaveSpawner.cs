using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnemySpawnInfo
{
    public GameObject enemyPrefab;
    public Transform spawnPoint;
}

// A group of enemies that spawns together and must be fully defeated
// before the next wave starts. A wave can have an optional reward (for
// example a weapon upgrade) that appears once the wave is cleared, so the
// level's progression never depends on random drops.
[Serializable]
public class Wave
{
    public string waveName = "Wave";
    public EnemySpawnInfo[] enemiesToSpawn;
    public float delayBeforeWave = 1f;
    public GameObject clearReward;
    public Transform rewardSpawnPoint;
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
    public static event Action<GameObject> OnRewardSpawned;

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

            SpawnReward(wave);
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

    private void SpawnReward(Wave wave)
    {
        if (wave.clearReward == null || wave.rewardSpawnPoint == null) return;

        GameObject reward = Instantiate(wave.clearReward, wave.rewardSpawnPoint.position, Quaternion.identity);
        OnRewardSpawned?.Invoke(reward);
    }

    private void HandleEnemyDied(EnemyHealth enemy)
    {
        aliveEnemies.Remove(enemy);
    }
}
