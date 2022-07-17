using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatManager : MonoBehaviour
{
    [System.Serializable]
    public class Stat
    {
        public StatInfo info;
        public int numericalValue;
    }

    [System.Serializable]
    public class StatInfo
    {
        public Sprite icon;
        public string id;
        public string displayName;
        public string description;
        public bool ignoreNumber;
        public bool hidePercentage;
        public string percentSuffix = "chance";
        public bool remainInPoolAfterSelected;
        public float chanceToChoose = 1;
        public int minimumRoundToAppear;
    }

    // Stat pools that can appear on dice
    public StatInfo[] basicStats;
    public StatInfo[] weaponModifiers;
    public StatInfo[] greatstuff;
    public StatInfo[] terriblestuff;
    public StatInfo[] numericalDice;
    public StatInfo[] enemyDice;

    public Sprite[] numbers;

    public List<Stat> playerStats;
    public List<Stat> enemyStats;
    public List<string> previouslyRolledStatIds;

    StatDisplayer[] playerStatDisplayers;
    List<StatDisplayer> enemyStatDisplayers;
    Image playerStatsBg;
    WaveManager wm;

    // Start is called before the first frame update
    void Start()
    {
        wm = FindObjectOfType<WaveManager>();
        playerStats = new List<Stat>();
        enemyStats = new List<Stat>();
        playerStatsBg = GameObject.Find("PlayerStatsBG").GetComponent<Image>();
        playerStatDisplayers = new StatDisplayer[11];
        enemyStatDisplayers = new List<StatDisplayer>();

        for (int i = 0; i < 10; i++)
            playerStatDisplayers[i] = playerStatsBg.transform.GetChild(i).GetComponent<StatDisplayer>();

        for (int i = 10; i < 25; i++)
            enemyStatDisplayers.Add(playerStatsBg.transform.GetChild(i).GetComponent<StatDisplayer>());
    }

    public bool PlayerHasStat(string id)
    {
        for (int i = 0; i < playerStats.Count; i++)
            if (playerStats[i].info.id.Equals(id))
                return true;
        return false;
    }

    public bool EnemyHasStat(string id)
    {
        for (int i = 0; i < enemyStats.Count; i++)
            if (enemyStats[i].info.id.Equals(id))
                return true;
        return false;
    }

    public void AddPlayerStat(Stat s)
    {
        if (playerStats.Count >= 11)
            return;

        playerStats.Add(s);
        playerStatDisplayers[playerStats.Count - 1].UpdateDisplayedStat(s, playerStats.Count-1);
    }

    public void AddEnemyStat(Stat s)
    {
        enemyStats.Add(s);
        enemyStatDisplayers[enemyStats.Count - 1].UpdateDisplayedStat(s, enemyStats.Count-1);
    }

    public void ReplacePlayerStat(int index, Stat newStat)
    {
        playerStats[index] = newStat;
        playerStatDisplayers[index].UpdateDisplayedStat(newStat, index);
    }

    public void ReplaceEnemyStat(int index, Stat newStat)
    {
        enemyStats[index] = newStat;
        enemyStatDisplayers[index].UpdateDisplayedStat(newStat, index);
    }

    // Gets a random stat from our stat pool and gives it a random number value
    public Stat GetNewStat(StatInfo[] statPool)
    {
        Stat s = new Stat();

        int rand = Random.Range(0, statPool.Length);
        float pickupChance = Random.Range(0, 1f);
        while (pickupChance >= statPool[rand].chanceToChoose || HasRolledStatPreviously(statPool[rand].id) || statPool[rand].minimumRoundToAppear > wm.currentRound)
        {
            rand = Random.Range(0, statPool.Length);
        }

        s.info = statPool[rand];
        if (!s.info.remainInPoolAfterSelected)
            previouslyRolledStatIds.Add(s.info.id);

        s.numericalValue = Random.Range(0, 6) + 1;
        return s;
    }

    public bool HasRolledStatPreviously(string id)
    {
        for (int i = 0; i < previouslyRolledStatIds.Count; i++)
            if (previouslyRolledStatIds[i].Contains(id))
                return true;
        return false;
    }

    public StatInfo[] GetPoolFromPrefix(string id)
    {
        if (id.Contains("stats"))
            return basicStats;
        if (id.Contains("mods"))
            return weaponModifiers;
        if (id.Contains("greatstuff"))
            return greatstuff;
        if (id.Contains("badstuff"))
            return terriblestuff;
        if (id.Contains("mob"))
            return enemyDice;

        return null;
    }
}
