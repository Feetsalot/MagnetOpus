using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] private GameObject endScreen;

    [SerializeField] private int currentRound;

    [SerializeField] private int maxRound;

    [SerializeField] private string nextLevel;

    [SerializeField] private GameObject winScreen;

    [SerializeField] private List<GameObject> metalFragPrefabs;

    [SerializeField] private List<MetalFrag> metalFragObjects;
    [SerializeField] private int maxMetalFrags;

    public int CurrentRound
    {
        get { return currentRound; }
    }


    public enum GameState
    {
        playing,
        paused
    }
    private GameState currentGameState;
    public GameState CurrentGameState
    {
        get { return currentGameState; }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null) instance = this;
        metalFragObjects = new List<MetalFrag>();
        currentGameState = GameState.playing;
    }

    public void RemoveMetalFrag(MetalFrag aFragToRemove)
    {
        if (!metalFragObjects.Contains(aFragToRemove)) return;
        metalFragObjects.Remove(aFragToRemove);
        metalFragObjects.TrimExcess();
    }

    public MetalFrag SpawnMetalFrag(Vector3 aPos, Quaternion angle)
    {
        if(metalFragObjects.Count >= maxMetalFrags)
        {
            GameObject lRemoving = metalFragObjects[0].gameObject;
            metalFragObjects.RemoveAt(0);
            metalFragObjects.TrimExcess();
            Destroy(lRemoving);
        }
        int randPrefab = Random.Range(0, metalFragPrefabs.Count);
        GameObject frag = Instantiate(metalFragPrefabs[randPrefab], aPos, angle);
        MetalFrag lFragToAdd = frag.GetComponent<MetalFrag>();
        metalFragObjects.Add(lFragToAdd);
        return frag.GetComponent<MetalFrag>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void IncrementRound()
    {
        currentRound++;
        if(currentRound > maxRound)
        {
            winScreen.SetActive(true);
            currentGameState = GameState.paused;
        }
    }

    public void LoadNextLevel()
    {
        SceneManager.LoadSceneAsync(nextLevel, LoadSceneMode.Single);
    }

    public void LoseLevel()
    {
        endScreen.SetActive(true);
        currentGameState = GameState.paused;
    }

    public void ReLoadLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadLevel(string aLevelName)
    {
        SceneManager.LoadSceneAsync(aLevelName, LoadSceneMode.Single);
    }
}
