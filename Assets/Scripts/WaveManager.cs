using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WaveManager : MonoBehaviour
{
    public int furthestRoundReached = 0;
    public int currentRound = 1;
    public int currentWorld = 1;
    public int currentLoop = 0;

    public bool inBattle = true;

    public List<EnemyGroup> enemiesToSpawn;
    public List<GameObject> additionalEnemies; // evil eye, reaper, etc.
    public List<Enemy> spawnedEnemies;
    public int enemiesLeft; // How many enemies we haven't spawned yet

    public Transform[] arenas;

    public GameObject tempSkeleton;

    GameManager gm;
    StatManager sm;
    PlayerController ply;

    int difficulty = 1;

    [System.Serializable]
    public class EnemyGroup
    {
        public GameObject enemy;
        public int numEnemies;

        public EnemyGroup(GameObject _enemy, int _numEnemies)
        {
            enemy = _enemy;
            numEnemies = _numEnemies;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        furthestRoundReached = currentRound + (currentWorld - 1) * 3 + currentLoop * 9;
        gm = FindObjectOfType<GameManager>();
        sm = FindObjectOfType<StatManager>();
        ply = FindObjectOfType<PlayerController>();
        spawnedEnemies = FindObjectsOfType<Enemy>().ToList();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!inBattle)
            return;

        spawnedEnemies.RemoveAll(item => item == null);

        if (spawnedEnemies.Count == 0 && enemiesLeft == 0)
        {
            StopAllCoroutines();
            IncreaseRound();
            gm.StartCoroutine(gm.CompleteWaveSequence());
            inBattle = false;
        }
    }

    public void ResetGame()
    {
        inBattle = false;
        furthestRoundReached = 0;
        currentRound = 1;
        currentWorld = 1;
        currentLoop = 0;
    }

    IEnumerator SpawnEnemies()
    {
        int multiplierMax = 1;
        if (sm.EnemyHasStat("badstuff_double"))
            multiplierMax = 2;

        // Determine how many total enemies we'll spawn
        enemiesLeft = additionalEnemies.Count;
        for (int i = 0; i < enemiesToSpawn.Count; i++)
            enemiesLeft += enemiesToSpawn[i].numEnemies * multiplierMax;

        // Spawn extra enemies at start of match
        for (int i = 0; i < additionalEnemies.Count; i++)
        {
            Instantiate(additionalEnemies[i], new Vector2(Random.Range(-20, 21), Random.Range(16, 21)), Quaternion.identity);
            yield return new WaitForSeconds(0.2f);
            enemiesLeft--;
        }

        int timeMultiplier = Mathf.Clamp(21 - difficulty, 3, 21);
        for (int i = 0; i < enemiesToSpawn.Count; i++)
        {
            int groupSize = enemiesToSpawn[i].numEnemies;
            int roomSide = 1;
            if (ply.transform.position.x >= 0)
                roomSide = -1;
            Vector2 clusterPoint = new Vector2(roomSide * Mathf.RoundToInt(Random.Range(18, 21f) / 2) * 2, Mathf.RoundToInt(Random.Range(-18, 21) / 2) * 2);
            for (int j = 0; j < groupSize * multiplierMax; j++)
            {
                Vector2 offset = new Vector2(Mathf.RoundToInt(Random.Range(-4, 5f) / 2) * 2, Mathf.RoundToInt(Random.Range(-4, 5f) / 2) * 2);
                GameObject g = Instantiate(enemiesToSpawn[i].enemy, clusterPoint + offset, Quaternion.identity);
                spawnedEnemies.Add(g.GetComponent<Enemy>());
                enemiesLeft--;
                if (i != groupSize - 1)
                    yield return new WaitForSeconds(0.2f);
            }

            yield return new WaitForSeconds(difficulty / 3);
        }
    }

    public void PrepareEnemyWave()
    {
        enemiesToSpawn.Clear();
        additionalEnemies.Clear();
        for (int i = 0; i < sm.enemyStats.Count; i++)
        {
            // Standard enemy load
            if (sm.enemyStats[i].info.id.Contains("mob"))
            {
                int counter = 1;
                if (!sm.enemyStats[i].info.ignoreNumber)
                    counter = sm.enemyStats[i].numericalValue;

                print("Added");
                enemiesToSpawn.Add(new EnemyGroup(GetEnemyFromID(sm.enemyStats[i].info.id.Substring(4)), counter));
            }

            // Special cases
            else
            {
                if (sm.enemyStats[i].info.id.Contains("badstuff_Marble") || sm.enemyStats[i].info.id.Contains("badstuff_Reaper"))
                    additionalEnemies.Add(GetEnemyFromID(sm.enemyStats[i].info.id.Substring(9)));
            }
        }
    }

    // Checks if all remaining enemies are invincible
    public bool AllEnemiesInvincible()
    {
        for (int i = 0; i < spawnedEnemies.Count; i++)
            if (!spawnedEnemies[i].stats.invincible)
                return false;
        return true;
    }

    GameObject GetEnemyFromID(string enemyName)
    {
        GameObject g = (GameObject)Resources.Load(@"Enemies/" + enemyName);
        return g;
    }

    public void UpdateArena()
    {
        for (int i = 0; i < 3; i++)
        {
            if (currentWorld - 1 == i)
                arenas[i].position = new Vector2(-25, 25);
            else
                arenas[i].position = Vector2.right * 300;
        }
    }

    public void StartNextWave()
    {
        // Get list of enemies from bogus menu
        // Spawn gradually over course of wave
        PrepareEnemyWave();
        StartCoroutine(SpawnEnemies());
        inBattle = true;
    }

    void IncreaseRound()
    {
        furthestRoundReached++;
        currentRound++;
        if (currentRound >= 4)
        {
            currentRound = 1;
            currentWorld++;
            if (currentWorld >= 4)
            {
                currentWorld = 1;
                currentLoop++;
            }
        }
    }

    public string CurrentWaveToString()
    {
        string s = currentWorld + "-" + currentRound;
        if (currentLoop > 0)
            s += "   Loop " + currentLoop;

        return s;
    }
}
