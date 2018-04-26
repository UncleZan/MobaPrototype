using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour {

    private const int MAX_ENEMIES_ON_STAGE = 20;

    float positionX, positionY;

    public static PoolOfObjects foesPool;
    public Transform foesContainer;
    public GameObject foePrefab;

    public static PoolOfObjects enemyPrjPool;
    public Transform enemyPrjContainer;
    public GameObject enemyPrjPrefab;

    public static int enemiesKilled = 0;
    public static int enemiesToKill = 0;
    public static int enemiesSpawned = 0;
    public static int stageLevel = 1;

    public Transform terrain;
    /// <summary>
    /// Determines how much time before the next foe is spawned
    /// </summary>
    float spawnDelay = 3.5f;

    // Use this for initialization
    void Start () {
        if (foesPool == null)
            foesPool = new PoolOfObjects(foesContainer, foePrefab);
        if (enemyPrjPool == null)
            enemyPrjPool = new PoolOfObjects(enemyPrjContainer, enemyPrjPrefab);
        enemiesToKill = 1;
        SpawnFoe();
    }

    // Update is called once per frame
    void Update () {
        if (enemiesKilled == enemiesToKill)
            InitNewStage();
    }

    //Generates a new foe in the stage
    void SpawnFoe()
    {
        int typeRndFactor = Random.Range(0, 2);
        positionX = Random.Range(-terrain.GetComponent<SpriteRenderer>().size.x, terrain.GetComponent<SpriteRenderer>().size.x);
        positionY = Random.Range(-terrain.GetComponent<SpriteRenderer>().size.y, terrain.GetComponent<SpriteRenderer>().size.y);


        GameObject newFoe = foesPool.GetObject();
        newFoe.transform.position = new Vector2(positionX, positionY);
        enemiesSpawned++;
        if (enemiesSpawned < enemiesToKill && enemiesSpawned < MAX_ENEMIES_ON_STAGE)
            StartCoroutine("SpawnNextFoe");
    }

    IEnumerator SpawnNextFoe()
    {
        yield return new WaitForSeconds(spawnDelay);
        if (enemiesSpawned < enemiesToKill && enemiesSpawned < MAX_ENEMIES_ON_STAGE)
            SpawnFoe();
    }

    void InitNewStage()
    {
        enemiesToKill += 1;
        if (spawnDelay > 0.1f)
            spawnDelay -= 0.1f;
        stageLevel++;
        SpawnFoe();
        Debug.Log("Stage " + stageLevel);
    }

    public static void KillEnemy(GameObject enemy)
    {
        if (enemy.activeSelf)
        {
            foesPool.DisableObject(enemy);
            enemiesKilled++;
        }
    }
}
