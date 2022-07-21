using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using TMPro;
using System.IO;

public class GameManager : MonoBehaviour
{
    [System.Serializable]
    public class GameVariables
    {
        public bool gamePaused;
        public bool isLoadingLevel;
        public Vector2 screenSize;
        public UIState currentUIState;
        public int kills;
    }
    [System.Serializable]
    public class GamePublicReferences
    {
        public Transform globalCameraHolderReference;
        public AudioMixer mixer;
        public GameObject hitNumber;
        public GameObject[] smokeClouds;
        public GameObject bigExplosion;
        public GameObject lightningChain;
        public GameObject lightningBall;
        public GameObject fire;
    }
    [System.Serializable]
    public class GameSoundEffects
    {
        public AudioClip[] generalSfx;
        public AudioClip[] playerSfx;
        public AudioClip[] uiSfx;
        public AudioClip[] musicTracks;
    }
    [System.Serializable]
    public class GameSaveData
    {
        // TBD
    }

    // UI states
    public enum UIState
    {
        titleScreen,
        inGame,
        settings,
        cutscene
    }

    // Main class references
    public GameVariables gm_gameVars;
    public GamePublicReferences gm_gameRefs;
    public GameSoundEffects gm_gameSfx;
    public GameSaveData gm_gameSaveData;

    // Audio references
    AudioSource musicSource;
    AudioSource ambienceSource;
    AudioSource sfxSource;
    AudioSource sfxSourceStoppable;

    // UI references
    Image blackScreenOverlay;
    Slider sfxSlider;
    Slider musicSlider;
    Slider ambSlider;
    StatDisplayer combinedRandomDiceStatDisplayer;
    TextMeshProUGUI diceRollText;
    TextMeshProUGUI nextRoundTitleText;

    Image statBg;
    RectTransform levelUpPanel;
    Animator diceRollPanel;
    RectTransform playerStatsPanel;

    RectTransform hudHolder;
    RectTransform nextLevelButton;
    RectTransform goBackPanel;
    RectTransform attributeSelectPanel;
    Image statEffectImage;
    Image statValueImage;

    Button statBoostButton;
    Button weaponModButton;
    Button statEffectButton;

    RectTransform gameOverpanel;
    RectTransform titleScreenPanel;
    TextMeshProUGUI totalKillsText;
    TextMeshProUGUI totalRoomsText;
    TextMeshProUGUI healthText;

    // Other references
    NewgroundsUtility ng;
    Transform cam;
    CameraFollow camFollow;
    PlayerController ply;
    StatManager sm;
    WaveManager wm;
    RandomizedDiceDisplayer[] randDice;
    StatManager.Stat newStatToDisplay;

    // Local variables
    bool initialized;
    int endOfLevelSelection = -1;
    [HideInInspector]
    public StatDisplayer displayerToReroll;
    [HideInInspector]
    public int rerollDepth = 0;

    void Start()
    {
        DontDestroyOnLoad(transform.parent.gameObject);
        GetReferences();
        //LoadAudioLevelsFromPlayerPrefs();
        blackScreenOverlay.color = Color.black;
        initialized = true;

        if (SceneManager.GetActiveScene().buildIndex == 1)
            LoadLevel(2);
        else if (SceneManager.GetActiveScene().buildIndex == 3)
        {
            StartCoroutine(FadeFromBlack());
            blackScreenOverlay.color = Color.clear;
            PostLoadUpdates(3);
        }

    }
    void Update()
    {
        if (!initialized)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
            SetFullscreenMode(!Screen.fullScreen);

        if (gm_gameVars.currentUIState == UIState.inGame)
            healthText.text = ply.stats.health.ToString();
    }

    public void CheckAndPlayClip(string clipName, Animator anim)
    {
        if (!anim.GetCurrentAnimatorStateInfo(0).IsName(clipName))
        {
            anim.Play(clipName);
        }
    } // Plays animation clip if it isn't already playing
    public IEnumerator FadeFromBlack()
    {
        blackScreenOverlay.rectTransform.anchoredPosition = Vector2.zero;
        while (blackScreenOverlay.color.a > 0)
        {
            blackScreenOverlay.color = new Color(0, 0, 0, blackScreenOverlay.color.a - 0.075f);
            yield return new WaitForSecondsRealtime(0.025f);
        }
        blackScreenOverlay.rectTransform.anchoredPosition = Vector2.up * 1000;
    }
    public IEnumerator FadeToBlack()
    {
        blackScreenOverlay.rectTransform.anchoredPosition = Vector2.zero;
        while (blackScreenOverlay.color.a < 1)
        {
            blackScreenOverlay.color = new Color(0, 0, 0, blackScreenOverlay.color.a + 0.075f);
            yield return new WaitForSecondsRealtime(0.025f);
        }
    }
    void GetReferences()
    {
        cam = gm_gameRefs.globalCameraHolderReference.GetChild(0);
        camFollow = FindObjectOfType<CameraFollow>();
        sm = GetComponent<StatManager>();

        // UI references
        blackScreenOverlay = GameObject.Find("BlackScreenOverlay").GetComponent<Image>();
        sfxSlider = GameObject.Find("SFXVolumeSlider").GetComponent<Slider>();
        ambSlider = GameObject.Find("AmbienceVolumeSlider").GetComponent<Slider>();
        musicSlider = GameObject.Find("MusicVolumeSlider").GetComponent<Slider>();

        statBg = GameObject.Find("StatBG").GetComponent<Image>();
        levelUpPanel = GameObject.Find("LevelUpPanel").GetComponent<RectTransform>();
        playerStatsPanel = GameObject.Find("PlayerStatsPanel").GetComponent<RectTransform>();
        diceRollPanel = GameObject.Find("DiceRollPanel").GetComponent<Animator>();
        randDice = new RandomizedDiceDisplayer[2];
        randDice[0] = GameObject.Find("LeftDiceIcon").GetComponent<RandomizedDiceDisplayer>();
        randDice[1] = GameObject.Find("RightDiceIcon").GetComponent<RandomizedDiceDisplayer>();
        combinedRandomDiceStatDisplayer = GameObject.Find("CombinedDiceStatDisplayer").GetComponent<StatDisplayer>();
        diceRollText = GameObject.Find("DiceRollText").GetComponent<TextMeshProUGUI>();
        nextRoundTitleText = GameObject.Find("NextRoundTitleText").GetComponent<TextMeshProUGUI>();
        nextLevelButton = GameObject.Find("NextLevelButton").GetComponent<RectTransform>();
        goBackPanel = GameObject.Find("GoBackPanel").GetComponent<RectTransform>();
        attributeSelectPanel = GameObject.Find("SelectDiceAttributePanel").GetComponent<RectTransform>();
        statEffectImage = GameObject.Find("StatEffectButton").GetComponent<Image>();
        statValueImage = GameObject.Find("StatValueButton").GetComponent<Image>();
        statBoostButton = GameObject.Find("NewStatButton").GetComponent<Button>();
        weaponModButton = GameObject.Find("NewWeaponModButton").GetComponent<Button>();
        statEffectButton = GameObject.Find("StatEffectButton").GetComponent<Button>();
        healthText = GameObject.Find("HealthText").GetComponent<TextMeshProUGUI>();
        gameOverpanel = GameObject.Find("GameOverPanel").GetComponent<RectTransform>();
        hudHolder = GameObject.Find("HUDHolder").GetComponent<RectTransform>();
        titleScreenPanel = GameObject.Find("TitleScreenPanel").GetComponent<RectTransform>();
        totalKillsText = GameObject.Find("TotalKillsText").GetComponent<TextMeshProUGUI>();
        totalRoomsText = GameObject.Find("TotalWavesText").GetComponent<TextMeshProUGUI>();

        // Audio references
        musicSource = GameObject.Find("GameMusicSource").GetComponent<AudioSource>();
        sfxSource = GameObject.Find("GameSFXSource").GetComponent<AudioSource>();
        sfxSourceStoppable = GameObject.Find("GameSFXSourceStoppable").GetComponent<AudioSource>();
        ambienceSource = GameObject.Find("GameAmbienceSource").GetComponent<AudioSource>();

        ng = FindObjectOfType<NewgroundsUtility>();

    } // Obtain UI + GameObject references. Called by Start() and probably nowhere else
    public void InitializePlayer() { } // Readies / unfreezes player gameobject in-game
    public void SetPausedState(bool paused) { } // Pauses / unpauses game and performs necessary UI stuff
    public void UnlockMedal(int id)
    {
        ng.UnlockMedal(id);
    } // Newgrounds API, self-explanatory
    public void LoadLevel(int buildIndex)
    {
        if (gm_gameVars.isLoadingLevel)
            return;

        gm_gameVars.isLoadingLevel = true;
        StartCoroutine(LoadLevelCoroutine(buildIndex));
    }
    IEnumerator LoadLevelCoroutine(int buildIndex)
    {
        yield return FadeToBlack();
        AsyncOperation asyncLoadLevel = SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Single);
        while (!asyncLoadLevel.isDone)
            yield return null;

        PostLoadUpdates(buildIndex);
        yield return FadeFromBlack();
        gm_gameVars.isLoadingLevel = false;
    }
    void PostLoadUpdates(int buildIndex)
    {
        if (buildIndex == 2)
        {
            gm_gameVars.currentUIState = UIState.titleScreen;
            titleScreenPanel.anchoredPosition = Vector2.zero;
            PlayMusic(2);
            hudHolder.anchoredPosition = Vector2.down * 1500;
            gameOverpanel.anchoredPosition = Vector2.down * 1500;
            statBg.color = Color.clear;
            if (Application.platform == RuntimePlatform.WindowsPlayer)
                GameObject.Find("ExitGameButton").GetComponent<RectTransform>().anchoredPosition = Vector2.one * 64;
        }
        else if (buildIndex == 3)
        {
            PlayMusic(Random.Range(0, 2));
            gm_gameVars.currentUIState = UIState.inGame;
            titleScreenPanel.anchoredPosition = Vector2.down * 1500;
            hudHolder.anchoredPosition = Vector2.zero;
            gameOverpanel.anchoredPosition = Vector2.down * 1500;
            StartCoroutine(InitializeGame());
        }

    } // Updates / changes that are performed when a level is loaded

    public void QuitGame()
    {
        Application.Quit();
    }

    public void OpenMountainLink()
    {
        Application.OpenURL("https://the-mountain.itch.io/");
    }

    void LoadAudioLevelsFromPlayerPrefs()
    {
        if (PlayerPrefs.HasKey("AMB_VOLUME"))
        {
            musicSlider.value = PlayerPrefs.GetInt("MUS_VOLUME") / 4;
            sfxSlider.value = PlayerPrefs.GetInt("SFX_VOLUME") / 4;
            ambSlider.value = PlayerPrefs.GetInt("AMB_VOLUME") / 4;
        }

        UpdateMusicVolume();
        UpdateSFXVolume();
        UpdateAmbienceVolume();
    } // Sets audio levels to match stored values in PlayerPrefs
    public void PlaySFX(AudioClip sfx)
    {
        sfxSource.PlayOneShot(sfx);
    }

    public void PlaySFXStoppable(AudioClip sfx)
    {
        sfxSourceStoppable.Stop();
        sfxSourceStoppable.PlayOneShot(sfx);
    }

    public void PlayMusic(int index)
    {
        musicSource.Stop();
        musicSource.clip = gm_gameSfx.musicTracks[index];
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }
    void ReadSaveData()
    {
        string prefix = @"idbfs/" + Application.productName;

        if (Application.platform == RuntimePlatform.WindowsPlayer)
            prefix = Application.persistentDataPath;

        if (!File.Exists(prefix + @"/savedata.json"))
        {
            gm_gameSaveData = new GameSaveData();
            return;
        }
        string json = File.ReadAllText(prefix + @"/savedata.json");
        gm_gameSaveData = JsonUtility.FromJson<GameSaveData>(json);
    }
    void SetFullscreenMode(bool isFullscreen)
    {
        int width = (int)gm_gameVars.screenSize.x;
        int height = (int)gm_gameVars.screenSize.y;
        if (isFullscreen)
        {
            width = Screen.currentResolution.width;
            height = Screen.currentResolution.height;
        }

        Screen.SetResolution(width, height, isFullscreen);
    } // Should work fine without AspectRatioController
    public void ScreenShake()
    {
        StartCoroutine(ScreenShakeCoroutine(5));
    }
    public void ScreenShake(float intensity)
    {
        StartCoroutine(ScreenShakeCoroutine(intensity));
    }
    IEnumerator ScreenShakeCoroutine(float intensity)
    {
        for (int i = 0; i < 10; i++)
        {
            cam.localPosition = new Vector2(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f)) * intensity;
            intensity /= 1.25f;
            yield return new WaitForFixedUpdate();
        }
        cam.localPosition = Vector2.zero;
    }
    public void UpdateAmbienceVolume()
    {
        if (ambSlider == null)
            return;

        int volume = (int)ambSlider.value * 4;

        if (volume == -40)
            volume = -80;

        PlayerPrefs.SetInt("AMB_VOLUME", volume);
        gm_gameRefs.mixer.SetFloat("AmbienceVolume", volume);
    }
    public void UpdateMusicVolume()
    {
        if (musicSlider == null)
            return;

        int volume = (int)musicSlider.value * 4;

        if (volume == -40)
            volume = -80;

        PlayerPrefs.SetInt("MUS_VOLUME", volume);
        gm_gameRefs.mixer.SetFloat("MusicVolume", volume);
    }
    public void UpdateSFXVolume()
    {
        if (sfxSlider == null)
            return;

        int volume = (int)sfxSlider.value * 4;

        if (volume == -40)
            volume = -80;

        PlayerPrefs.SetInt("SFX_VOLUME", volume);
        gm_gameRefs.mixer.SetFloat("SFXVolume", volume);
    }
    void WriteSaveData()
    {
        // Reason for idbfs prefix: https://itch.io/t/140214/persistent-data-in-updatable-webgl-games (don't question it)
        string prefix = @"idbfs/" + Application.productName;

        if (Application.platform == RuntimePlatform.WindowsPlayer)
            prefix = Application.persistentDataPath;
        else if (!Directory.Exists(prefix))
            Directory.CreateDirectory(prefix);

        string json = JsonUtility.ToJson(gm_gameSaveData);
        File.WriteAllText(prefix + @"/savedata.json", json);
    }
    public void RetryGame()
    {
        LoadLevel(3);
    }

    IEnumerator InitializeGame()
    {
        ply = FindObjectOfType<PlayerController>();
        wm = FindObjectOfType<WaveManager>();
        camFollow.GetRefs();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        ResetGame();

        while (blackScreenOverlay.color.a > 0)
            yield return null;

        diceRollPanel.Play("DiceRoller_PushIn", 0, 0);

        playerStatsPanel.anchoredPosition = Vector2.zero;
        statBg.color = Color.white;

        // Add new enemies to lineup
        diceRollText.text = "YOUR FIRST ENEMIES";
        yield return RollBothDice(sm.enemyDice, 1);
        yield return RollBothDice(sm.enemyDice, 1);
        yield return RollBothDice(sm.enemyDice, 1);
        yield return RollBothDice(sm.enemyDice, 1);
        yield return RollBothDice(sm.enemyDice, 1);
        yield return RollBothDice(sm.enemyDice, 1);
        yield return RollBothDice(sm.weaponModifiers, 0);
        yield return RollBothDice(sm.weaponModifiers, 0);
        yield return RollBothDice(sm.weaponModifiers, 0);
        yield return RollBothDice(sm.weaponModifiers, 0);

        nextRoundTitleText.text = "NEXT WAVE:\n" + wm.CurrentWaveToString();
        diceRollText.text = "";
        diceRollPanel.Play("DiceRoller_PushOut", 0, 0);
    }

    public IEnumerator EndLevel()
    {
        yield return new WaitForSeconds(1.25f);
        PlayMusic(3);
        totalKillsText.text = gm_gameVars.kills + "\nKILLS";
        totalRoomsText.text = wm.furthestRoundReached + " WAVES\nCLEARED";
        gameOverpanel.anchoredPosition = Vector2.zero;
    }

    public void ResetGame()
    {
        statBoostButton.interactable = true;
        weaponModButton.interactable = true;
        statEffectButton.interactable = true;
        levelUpPanel.anchoredPosition = Vector2.down * 1500;
        playerStatsPanel.anchoredPosition = Vector2.down * 1500;
        attributeSelectPanel.anchoredPosition = Vector2.down * 1500;
        goBackPanel.anchoredPosition = Vector2.down * 1500;
        statBg.color = Color.black;

        gm_gameVars.kills = 0;

        sm.ResetGame();
        wm.ResetGame();

        ply.canMove = false;
        ply.transform.position = Vector2.zero;
        ply.stats.health = ply.stats.baseMaxHealth;
        ply.isDead = false;
    }

    public IEnumerator CompleteWaveSequence()
    {
        ply.canMove = false;
        yield return new WaitForSeconds(1);

        float transparency = 0;
        while (statBg.color.a < 1)
        {
            transparency += Time.fixedDeltaTime * 4;
            statBg.color = new Color(statBg.color.r, statBg.color.g, statBg.color.b, transparency);
            yield return new WaitForFixedUpdate();
        }

        // Disable buttons if we've unlocked all stats or if we've reached the maximum number of boons
        if (sm.playerStats.Count >= 9)
        {
            statBoostButton.interactable = false;
            weaponModButton.interactable = false;
        }

        if (sm.unlockedStatBuffs.Count == sm.basicStats.Length)
            statBoostButton.interactable = false;
        if (sm.unlockedWeaponMods.Count == sm.weaponModifiers.Length)
            weaponModButton.interactable = false;

        levelUpPanel.anchoredPosition = Vector2.zero;

        // Value is determined by player input
        // Waits until selection is made
        while (endOfLevelSelection == -1)
            yield return null;

        levelUpPanel.anchoredPosition = Vector2.down * 1500;
        diceRollPanel.Play("DiceRoller_PushIn", 0, 0);

        yield return new WaitForSeconds(0.25f);

        // Add player buff after determining pool
        StatManager.StatInfo[] pool = null;
        if (endOfLevelSelection == 0) // New stat
            pool = sm.basicStats;
        else if (endOfLevelSelection == 1) // New weapon mod
            pool = sm.weaponModifiers;

        if (endOfLevelSelection < 2)
        {
            diceRollText.text = "ROLLING BOON";
            yield return RollBothDice(pool, 0); // 0 for player, 1 for enemy
        }
        else
        {
            string previousId = "";
            if (displayerToReroll != null)
                previousId = displayerToReroll.displayedStat.info.id;
            diceRollText.text = "REROLLING";

            int recipient = 0;
            print(displayerToReroll == null);
            print(displayerToReroll.displayedStat == null);
            print(displayerToReroll.displayedStat.info.id);
            if (displayerToReroll.displayedStat.info.id.Contains("mob_") || displayerToReroll.displayedStat.info.id.Contains("badstuff_"))
                recipient = 1;

            // Reroll left die
            if (endOfLevelSelection == 2)
                yield return RollSingleDie(sm.GetPoolFromPrefix(displayerToReroll.displayedStat.info.id), recipient, 0);

            // Reroll right die
            else if (endOfLevelSelection == 3)
                yield return RollSingleDie(sm.numericalDice, recipient, 1);

            // Reroll single center die
            else if (endOfLevelSelection == 4)
                yield return RollCenterDie(sm.GetPoolFromPrefix(displayerToReroll.displayedStat.info.id), recipient);

            sm.RemoveStatFromPreviouslyRolledPool(previousId);
        }

        rerollDepth = 0;
        endOfLevelSelection = -1;
        displayerToReroll = null;

        // Add goodstuff if we've looped
        if (wm.currentLoop == 1 && wm.currentRound == 1 && wm.currentWorld == 1)
        {
            diceRollText.text = "YOUR REWARD\nFOR LOOPING";
            yield return RollCenterDie(sm.goodstuff, 0);
        }

        // Add new enemies to lineup
        diceRollText.text = "ROLLING BOGUS";
        yield return RollBothDice(sm.enemyDice, 1);

        if (sm.enemyStats.Count < 15)
        {
            // Add enemy badstuff if new world reached
            if (wm.currentRound == 1 && !(wm.currentLoop == 0 && wm.currentWorld == 1))
            {
                diceRollText.text = "YOUR ENEMIES GROW STRONGER";
                yield return new WaitForSeconds(0.25f);
                yield return RollCenterDie(sm.terriblestuff, 1);
            }
        }

        attributeSelectPanel.anchoredPosition = Vector2.down * 1500;
        goBackPanel.anchoredPosition = Vector2.down * 1500;
        nextLevelButton.anchoredPosition = Vector2.down * 290;
        nextRoundTitleText.text = "NEXT WAVE:\n" + wm.CurrentWaveToString();
        diceRollText.text = "";
        playerStatsPanel.anchoredPosition = Vector2.zero;

        // Update player stats buffs
        sm.PreparePlayerStats();

        diceRollPanel.Play("DiceRoller_PushOut", 0, 0);
    }

    IEnumerator RollBothDice(StatManager.StatInfo[] pool, int statRecipient)
    {
        PlaySFX(gm_gameSfx.generalSfx[2]);
        diceRollPanel.Play("DiceRoller_RollBoth", 0, 0);

        randDice[0].DisplayRandomIcons(pool);
        randDice[1].DisplayRandomIcons(sm.numericalDice);

        newStatToDisplay = sm.GetNewStat(pool);

        if (statRecipient == 0)
            sm.AddPlayerStat(newStatToDisplay, false);
        else
        {
            // Stop adding to displayers if we've filled up the space
            if (sm.enemyStats.Count >= 15)
                sm.enemyStats.Add(newStatToDisplay);
            else
                sm.AddEnemyStat(newStatToDisplay);
        }

        yield return new WaitForSeconds(2);
    }

    IEnumerator RollSingleDie(StatManager.StatInfo[] pool, int statRecipient, int dieToRoll)
    {
        PlaySFX(gm_gameSfx.generalSfx[2]);
        newStatToDisplay = displayerToReroll.displayedStat;

        if (dieToRoll == 0)
        {
            newStatToDisplay = sm.GetNewStat(pool);
            diceRollPanel.Play("DiceRoller_RollLeft", 0, 0);
            randDice[0].DisplayRandomIcons(pool);
            randDice[1].DisplaySingleIcon(sm.numericalDice[displayerToReroll.displayedStat.numericalValue - 1].icon);
            newStatToDisplay.numericalValue = displayerToReroll.displayedStat.numericalValue;
        }
        else
        {
            newStatToDisplay.numericalValue = Random.Range(1, 7);
            diceRollPanel.Play("DiceRoller_RollRight", 0, 0);
            randDice[0].DisplaySingleIcon(displayerToReroll.displayedStat.info.icon);
            randDice[1].DisplayRandomIcons(sm.numericalDice);
            newStatToDisplay.info = displayerToReroll.displayedStat.info;
        }

        if (statRecipient == 0)
            sm.ReplacePlayerStat(displayerToReroll.statIndex, newStatToDisplay, false);
        else
            sm.ReplaceEnemyStat(displayerToReroll.statIndex, newStatToDisplay);

        yield return new WaitForSeconds(2);
    }

    IEnumerator RollCenterDie(StatManager.StatInfo[] pool, int statRecipient)
    {
        PlaySFX(gm_gameSfx.generalSfx[2]);
        diceRollPanel.Play("DiceRoller_RollCenter", 0, 0);

        randDice[0].DisplayRandomIcons(pool);
        randDice[1].DisplayRandomIcons(sm.numericalDice);

        newStatToDisplay = sm.GetNewStat(pool);
        bool isUltra = newStatToDisplay.info.id.Contains("goodstuff");

        // Add a new die unless we're rerolling
        if (endOfLevelSelection < 2)
        {
            if (statRecipient == 0)
                sm.AddPlayerStat(newStatToDisplay, isUltra);
            else
            {
                // Stop adding to displayers if we've filled up the space
                if (sm.enemyStats.Count >= 15)
                    sm.enemyStats.Add(newStatToDisplay);
                else
                    sm.AddEnemyStat(newStatToDisplay);
            }
        }
        else
        {
            if (statRecipient == 0)
                sm.ReplacePlayerStat(displayerToReroll.statIndex, newStatToDisplay, isUltra);
            else
                sm.ReplaceEnemyStat(displayerToReroll.statIndex, newStatToDisplay);
        }

        yield return new WaitForSeconds(2);
    }

    public void DisplayRandDiceIcons()
    {
        // Display newly rolled icon on dice, but not for dice that aren't being rolled
        if (endOfLevelSelection != 3)
            randDice[0].DisplaySingleIcon(newStatToDisplay.info.icon);
        if (endOfLevelSelection != 2)
            randDice[1].DisplaySingleIcon(sm.numericalDice[newStatToDisplay.numericalValue - 1].icon);

        // Play appropriate SFX for result
        if (newStatToDisplay.info.id.Contains("badstuff"))
            PlaySFX(gm_gameSfx.generalSfx[1]);
        else if (newStatToDisplay.info.id.Contains("goodstuff"))
            PlaySFX(gm_gameSfx.generalSfx[0]);
        else
            PlaySFX(gm_gameSfx.generalSfx[3]);

        combinedRandomDiceStatDisplayer.UpdateDisplayedStat(newStatToDisplay, -1);
        newStatToDisplay = null;
    }

    public void SetDiceAttributeSprites()
    {
        print(displayerToReroll == null);
        print(displayerToReroll.name);

        statEffectButton.interactable = !((sm.unlockedStatBuffs.Count == 4 && displayerToReroll.displayedStat.info.id.Contains("stats")) || (sm.unlockedWeaponMods.Count == 7 && displayerToReroll.displayedStat.info.id.Contains("mods")));

        statEffectImage.sprite = displayerToReroll.displayedStat.info.icon;
        statValueImage.sprite = sm.numericalDice[displayerToReroll.displayedStat.numericalValue - 1].icon;
    }

    public void RerollDie(int die)
    {
        // 0: Reroll effect (left)
        // 1: Reroll value (right)
        // 2: Reroll both
        endOfLevelSelection = die + 2;
    }

    public void RerollAdvance()
    {
        if (rerollDepth == 0)
        {
            goBackPanel.anchoredPosition = Vector2.zero;
            playerStatsPanel.anchoredPosition = Vector2.zero;
            nextLevelButton.anchoredPosition = Vector2.down * 1500;
            levelUpPanel.anchoredPosition = Vector2.down * 1500;
            nextRoundTitleText.text = "SELECT A DIE\nTO REROLL";
        }
        else if (rerollDepth == 1)
        {
            // Cut to the chase and reroll both attributes if number is ignored
            if (displayerToReroll.displayedStat.info.ignoreNumber)
            {
                RerollDie(2);
            }
            else
            {
                playerStatsPanel.anchoredPosition = Vector2.down * 1500;
                attributeSelectPanel.anchoredPosition = Vector2.zero;
            }
        }

        rerollDepth++;
    }

    public void RerollBackUp()
    {
        // When back button is pressed:

        // Go back to level up screen
        if (rerollDepth == 1)
        {
            goBackPanel.anchoredPosition = Vector2.down * 1500;
            playerStatsPanel.anchoredPosition = Vector2.down * 1500;
            levelUpPanel.anchoredPosition = Vector2.zero;
        }

        // Go back to boon select screen
        if (rerollDepth == 2)
        {
            attributeSelectPanel.anchoredPosition = Vector2.down * 1500;
            playerStatsPanel.anchoredPosition = Vector2.zero;
        }

        rerollDepth--;
    }

    IEnumerator WaitForRerollSelection()
    {
        while (displayerToReroll == null)
            yield return null;

        endOfLevelSelection = 2;
    }

    public void StartNextWave()
    {
        StartCoroutine(StartNextWaveCoroutine());
    }

    IEnumerator StartNextWaveCoroutine()
    {
        wm.UpdateArena();
        playerStatsPanel.anchoredPosition = Vector2.right * 1500;
        ply.transform.position = new Vector2(0, 0.75f);
        float transparency = 1;
        while (statBg.color.a > 0)
        {
            transparency -= Time.fixedDeltaTime * 4;
            statBg.color = new Color(statBg.color.r, statBg.color.g, statBg.color.b, transparency);
            yield return new WaitForFixedUpdate();
        }

        wm.StartNextWave();
        ply.canMove = true;

    }

    public void SetSelection(int index)
    {
        endOfLevelSelection = index;
    }
}