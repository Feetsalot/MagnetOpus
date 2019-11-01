using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [System.Serializable]
    public struct Wave
    {
        public GameObject enemyToSpawn;
        public int amountToSpawn;
        public float indivualSpawnDelay;
    }

    [System.Serializable]
    public struct Round
    {
        public List<Wave> waves;
        public int roundNum;
    }

    public List<Round> rounds;

    public PathManager.Path pathToGive;
    public List<PathManager.Path> paths;

    [SerializeField] private Animator anim;
    [SerializeField] private Transform spawnLocation;

    [SerializeField] private GameObject spawnerArrow;

    private bool isSpawningWave;
    public bool IsSpawningWave
    {
        get { return isSpawningWave; }
    }

    private IEnumerator spawnWave(Wave aWaveToSpawn)
    {
        int lSpawnCounter = 0;
        while (lSpawnCounter < aWaveToSpawn.amountToSpawn)
        {
            GameObject lSpawnedEnemy = Instantiate(aWaveToSpawn.enemyToSpawn, spawnLocation.position, Quaternion.identity);
            EnemyWaveManager.instance.AddSpawnedEnemy(lSpawnedEnemy);
            EnemyBehaviour lPatrol = lSpawnedEnemy.GetComponent<EnemyBehaviour>();
            if (lPatrol != null)
            {
                lPatrol.SetPath(pathToGive);
            }
            lSpawnCounter++;
            yield return new WaitForSeconds(aWaveToSpawn.indivualSpawnDelay);
        }
        StartCoroutine(CloseHatch());
    }

    private IEnumerator CloseHatch()
    {
        if (anim != null) anim.SetBool("IsOpen", false);
        if(spawnerArrow != null)spawnerArrow.SetActive(false);
        yield return new WaitForSeconds(0.6f);
        isSpawningWave = false;
    }

    private IEnumerator OpenHatch(Wave w)
    {
        isSpawningWave = true;
        if (spawnerArrow != null) spawnerArrow.SetActive(true);
        if (anim != null) anim.SetBool("IsOpen", true);
        yield return new WaitForSeconds(1f);
        StartCoroutine(spawnWave(w));
    }

    public void StartRound(int lRoundNumber)
    {
        pathToGive = PathManager.instance.GetClosestActivePath((Vector2)transform.position);
        foreach (Round r in rounds)
        {
            if(r.roundNum == lRoundNumber)
            {
                foreach(Wave w in r.waves)
                {
                    StartCoroutine(OpenHatch(w));
                }
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
}
