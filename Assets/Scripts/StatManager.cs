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
        public StatInfo()
        {
            id = "";
        }

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
    public StatInfo[] goodstuff;
    public StatInfo[] terriblestuff;
    public StatInfo[] numericalDice;
    public StatInfo[] enemyDice;

    public Sprite[] numbers;

    public List<Stat> playerStats;
    public List<Stat> enemyStats;

    public List<Stat> unlockedStatBuffs;
    public List<Stat> unlockedWeaponMods;
    public Stat currentUltraStat;

    public List<string> previouslyRolledStatIds;

    StatDisplayer[] playerStatDisplayers;
    StatDisplayer playerUltraStatDisplayer;
    List<StatDisplayer> enemyStatDisplayers;
    Image playerStatsBg;
    WaveManager wm;
    PlayerController ply;

    // Start is called before the first frame update
    void Start()
    {
        playerStats = new List<Stat>();
        enemyStats = new List<Stat>();
        unlockedStatBuffs = new List<Stat>();
        unlockedWeaponMods = new List<Stat>();
        playerStatsBg = GameObject.Find("PlayerStatsBG").GetComponent<Image>();
        playerStatDisplayers = new StatDisplayer[9];
        enemyStatDisplayers = new List<StatDisplayer>();

        for (int i = 0; i < 9; i++)
            playerStatDisplayers[i] = playerStatsBg.transform.GetChild(i).GetComponent<StatDisplayer>();

        playerUltraStatDisplayer = playerStatsBg.transform.GetChild(9).GetComponent<StatDisplayer>();

        for (int i = 10; i < 25; i++)
            enemyStatDisplayers.Add(playerStatsBg.transform.GetChild(i).GetComponent<StatDisplayer>());
    }

    public void ResetGame()
    {
        ply = FindObjectOfType<PlayerController>();
        wm = FindObjectOfType<WaveManager>();
        playerStats.Clear();
        enemyStats.Clear();
        unlockedStatBuffs.Clear();
        unlockedWeaponMods.Clear();
        previouslyRolledStatIds.Clear();
        currentUltraStat.info = new StatManager.StatInfo();

        for (int i = 0; i < playerStatDisplayers.Length; i++)
            playerStatDisplayers[i].ClearDisplayedStat();
        for (int i = 0; i < enemyStatDisplayers.Count; i++)
            enemyStatDisplayers[i].ClearDisplayedStat();
        playerUltraStatDisplayer.ClearDisplayedStat();

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

    public void AddPlayerStat(Stat s, bool isUltra)
    {
        if (playerStats.Count >= 11)
            return;

        playerStats.Add(s);

        if (s.info.id.Contains("stats"))
            unlockedStatBuffs.Add(s);
        else if (s.info.id.Contains("mods"))
            unlockedWeaponMods.Add(s);

        if (!isUltra)
            playerStatDisplayers[playerStats.Count - 1].UpdateDisplayedStat(s, playerStats.Count - 1);
        else
        {
            playerUltraStatDisplayer.UpdateDisplayedStat(s, playerStats.Count - 1);
            currentUltraStat = s;
        }
    }

    public void AddEnemyStat(Stat s)
    {
        enemyStats.Add(s);
        enemyStatDisplayers[enemyStats.Count - 1].UpdateDisplayedStat(s, enemyStats.Count - 1);
    }

    public void ReplacePlayerStat(int index, Stat newStat, bool isUltra)
    {
        // Update statbuffs and weaponmods lists if id changed
        if (!playerStats[index].info.id.Equals(newStat.info.id))
        {
            if (newStat.info.id.Contains("stats"))
            {
                unlockedStatBuffs.Remove(playerStats[index]);
                unlockedStatBuffs.Add(newStat);
            }
            else if (playerStats[index].info.id.Contains("mods"))
            {
                unlockedWeaponMods.Remove(playerStats[index]);
                unlockedWeaponMods.Add(newStat);
            }
        }

        if (isUltra)
        {
            playerUltraStatDisplayer.UpdateDisplayedStat(newStat, index);
        }
        else
        {
            playerStats[index] = newStat;
            playerStatDisplayers[index].UpdateDisplayedStat(newStat, index);
        }
    }

    public void PreparePlayerStats()
    {
        for (int i = 0; i < unlockedStatBuffs.Count; i++)
        {
            float buff = 1 + unlockedStatBuffs[i].numericalValue / 12f;
            switch (unlockedStatBuffs[i].info.id)
            {
                case "stats_hp":
                    ply.buffs.healthMultiplier = buff;
                    break;
                case "stats_spd":
                    ply.buffs.speedMultiplier = buff;
                    break;
                case "stats_firerate":
                    ply.buffs.firerateMultiplier = buff;
                    break;
                case "stats_dmg":
                    ply.buffs.damageMultiplier = buff;
                    break;
            }
        }

        ply.stats.health = Mathf.RoundToInt(ply.stats.baseMaxHealth * ply.buffs.healthMultiplier);
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
        int counter = 0;
        while (pickupChance >= statPool[rand].chanceToChoose || HasRolledStatPreviously(statPool[rand].id) || statPool[rand].minimumRoundToAppear > wm.furthestRoundReached)
        {
            counter++;
            pickupChance = Random.Range(0, 1f);
            rand = Random.Range(0, statPool.Length);
            if (counter > 1000)
            {
                Debug.LogError("Couldn't find stat");
                break;
            }
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

    // If the player rolled a stat that can only appear once, but
    // now REROLLED its effect for something else, use this to put it back in the pool
    public void RemoveStatFromPreviouslyRolledPool(string id)
    {
        for (int i = 0; i < previouslyRolledStatIds.Count; i++)
        {
            if (previouslyRolledStatIds[i].Contains(id))
            {
                previouslyRolledStatIds.RemoveAt(i);
                break;
            }
        }
    }

    public StatInfo[] GetPoolFromPrefix(string id)
    {
        if (id.Contains("stats"))
            return basicStats;
        if (id.Contains("mods"))
            return weaponModifiers;
        if (id.Contains("goodstuff"))
            return goodstuff;
        if (id.Contains("badstuff"))
            return terriblestuff;
        if (id.Contains("mob"))
            return enemyDice;

        return null;
    }
}
