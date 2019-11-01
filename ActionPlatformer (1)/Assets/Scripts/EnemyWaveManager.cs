using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWaveManager : MonoBehaviour
{
    [SerializeField] private List<SpawnPoint> spawnPoints;

    [SerializeField] private List<GameObject> enemiesAlive;

    public static EnemyWaveManager instance;

    // Start is called before the first frame update
    void Start()
    {
        if(instance == null) instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if(!EnemiesLeft() && !IsSpawningWave())
        {
            GameManager.instance.IncrementRound();
            StartNextWave();
            FindObjectOfType<AudioHandler>().Play("Voice_WaveChange2");
        }
    }

    private bool IsSpawningWave()
    {
        foreach (SpawnPoint sp in spawnPoints)
        {
            if (sp.IsSpawningWave) return true;
        }
        return false;
    }

    private void StartNextWave()
    {
        foreach(SpawnPoint sp in spawnPoints)
        {
            sp.StartRound(GameManager.instance.CurrentRound);
        }
    }

    public void AddSpawnedEnemy(GameObject lEnemy)
    {
        enemiesAlive.Add(lEnemy);
    }

    public void RemoveEnemy(GameObject lEnemy)
    {
        enemiesAlive.Remove(lEnemy);
        enemiesAlive.TrimExcess();
    }

    private bool EnemiesLeft()
    {
        if (enemiesAlive.Count > 0)
        {
            return true;
        }
        return false;
    }
}
