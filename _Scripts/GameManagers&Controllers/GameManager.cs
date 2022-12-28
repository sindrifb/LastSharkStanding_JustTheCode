using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using Rewired;
using UnityEngine.Audio;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour {
    public List<GameObject> WinParticles;
    public bool UseBots;
    public PlayerDataModel Bot;
    public static GameManager Instance;
    public static float TimeSinceRoundStart;
    private bool IsPlaying;
    private float TimeBetweenDifficulty = 7;
    private float TimeToNextDifficulty = 0;
    private int DiffcultyByTimeIncreases = 0;
    public int RoundWinnerPlayerID { get; private set; } = 0;
    public int BotDifficulty = 1; // 0 = Easy, 1 = Medium, 2 = Hard
    public int Difficulty
    {
        get
        {
            return Mathf.Clamp(m_difficulty, 0, 10);
        }
        set
        {
            m_difficulty = value;
            //print("Difficulty: " + m_difficulty);
        }
    }

    private int m_difficulty = 0;
    public static bool GamePaused { get; private set; }

    public GameObject DestroyParticleEffect;
    public GameObject SpawnParticleEffect;
    public AudioMixer Mixer;

    public List<PlayerDataModel> PlayerDataModels = new List<PlayerDataModel>();
    public List<PlayerController> Players = new List<PlayerController>();
    //Powerups, traps, environmentals
    public List<GameObject> SpawnedObjects = new List<GameObject>();

    private AsyncOperation AsyncLoadLevel;
    private Coroutine LoadingCoroutine;
    public bool LoadingScene;
    public bool Tournament;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    private void OnLevelWasLoaded(int level)
    {
        GamePaused = false;
    }

    private void Start()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.unityLogger.logEnabled = true;
#else
        Debug.unityLogger.logEnabled = false;
#endif
        ChangeVolume(Constants.AudioMixerChannels.Music, PlayerPrefs.GetFloat(Constants.AudioMixerChannels.Music, 1));
        ChangeVolume(Constants.AudioMixerChannels.SFX, PlayerPrefs.GetFloat(Constants.AudioMixerChannels.SFX, 1));
    }

    IEnumerator SpawnBots()
    {
        yield return new WaitForSeconds(10f);
        var spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
        while (true)
        {
            if (UseBots)
            {
                for (int i = 0; i < 4 - PlayerDataModels.Count; i++)
                {
                    Vector3 spawnPos = Vector3.up / 2f;
                    var spawnRot = Quaternion.identity;
                    if (spawnPoints?.Length != 0 && spawnPoints[i + PlayerDataModels.Count] != null)
                    {
                        spawnPos = spawnPoints[i + PlayerDataModels.Count].transform.position;
                        spawnRot = spawnPoints[i + PlayerDataModels.Count].transform.rotation;
                    }

                    StartCoroutine(SpawnPlayer(Bot, spawnPos, spawnRot));
                }
            }
            yield return new WaitForSeconds(10f);
        }
    }

    private void Update()
    {
        if (IsPlaying)
        {
            TimeSinceRoundStart += Time.deltaTime;
            if (TimeSinceRoundStart > TimeToNextDifficulty)
            {
                Difficulty++;
                //DiffcultyByTimeIncreases++;
                //TimeToNextDifficulty = TimeSinceRoundStart + Mathf.Clamp((15 - (2 * DiffcultyByTimeIncreases)), 8, 30);
                TimeToNextDifficulty += TimeBetweenDifficulty;
            }
        }
    }

    public void StartGame()
    {
        //StartCoroutine(SpawnBots());
        ResetDifficulty();
        IsPlaying = true;
        
        StartCoroutine(ResetRoundStartUI());
        
        var endScreen = FindObjectOfType<GameEndUIController>();
        if (endScreen != null)
        {
            Destroy(endScreen.gameObject);
        }

        //MenusManager.Instance.DisableAllMenus();
        foreach (var player in PlayerDataModels.Where(a => a.RewiredID != (-1)).ToList())
        {
            player.RoundWins = 0;
            var maps = ReInput.players.GetPlayer(player.RewiredID).controllers.maps;
            maps.SetAllMapsEnabled(false);
            maps.SetMapsEnabled(true, "Gameplay");
        }

        if (FindObjectOfType<HazardManager>() == null)
        {
            HazardManager.Instance = null;
        }
        //SpawnPlayers();
        //EventManager.TriggerEvent(EventManager.EventCodes.RoundStart);
        //Debug.Log("GameStart triggered");
        EventManager.TriggerEvent(EventManager.EventCodes.GameStart);
    }

    private IEnumerator ResetRoundStartUI()
    {
        Destroy(UIManager.Instance.RoundStartScreen);

        yield return new WaitForEndOfFrame();

        UIManager.Instance.SetRoundStartScreenAsActive(true);
    }

    public void StartRound()
    {
        IsPlaying = true;
        //MenusManager.Instance.DisableAllMenus();
        //SpawnPlayers();

        for (int i = 0; i < Players.Count; i++)
        {
            Players[i].ChangeState(PlayerController.State.Idle);
        }
        //Debug.Log("RoundStart triggered");
        EventManager.TriggerEvent(EventManager.EventCodes.RoundStart);
    }

    public void ResetDifficulty()
    {
        DiffcultyByTimeIncreases = 0;
        TimeToNextDifficulty = TimeBetweenDifficulty;
        TimeSinceRoundStart = 0f;
        Difficulty = 0;
    }

    public void EndGame(PlayerDataModel pWinningPlayer)
    {
        IsPlaying = false;
        ResetDifficulty();
        for (int i = 0; i < ReInput.players.playerCount; i++)
        {
            var maps = ReInput.players.GetPlayer(i).controllers.maps;
            maps.SetAllMapsEnabled(false);
            maps.SetMapsEnabled(true, "Menus");
        }

        ClearArena();

        GameEndEvent endEvent = new GameEndEvent
        {
            Description = "Game End Event from GameManager",
            TournamentOver = false,
            RewiredID = pWinningPlayer.RewiredID,
            Winner = pWinningPlayer,
            PlayerDataModels = PlayerDataModels,
            PlayerScore = pWinningPlayer.RoundWins,
            Skin = pWinningPlayer.Skin.name,
            MapIndex = SceneManager.GetActiveScene().buildIndex,
            BotDifficulty = BotDifficulty
        };
        if (Tournament)
        {
            var tourny = FindObjectOfType<TournamentManager>();
            tourny.AddScore(pWinningPlayer);
            if (tourny.TournamentOver())
            {
                endEvent.Contenders = tourny.Contenders;
                endEvent.TournamentScoreNeeded = tourny.ScoreNeeded;
                endEvent.TournamentOver = true;
            }
        }
        
        UIManager.Instance.SpawnMatchEndScreen();
        // MenusManager.Instance.GoToMenu(1);
        //EventManager.TriggerEvent(EventManager.EventCodes.RoundEnd);
        //Debug.Log("GameEnd triggered");
        
        endEvent.FireEvent();
        EventManager.TriggerEvent(EventManager.EventCodes.GameEnd);
    }

    public void EndRound()
    {
        IsPlaying = false;
        ResetDifficulty();
        ClearArena();
        //var graterHazard = FindObjectsOfType<HazardHookable>();
        //if (graterHazard != null)
        //{
        //    for (int i = 0; i < graterHazard.Length; i++)
        //    {
        //        graterHazard[i].ResetHazard();
        //    }
        //}
        UIManager.Instance.SetRoundStartScreenAsActive(true);
        StartCoroutine(NextRoundTimer());
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.UISound.RoundStartIn5);
        //Debug.Log("RoundEnd triggered");
        EventManager.TriggerEvent(EventManager.EventCodes.RoundEnd);
    }

    IEnumerator NextRoundTimer()
    {
        float time = Time.time;
        while (Time.time - time < 5f)
        {
            //print the time on the count down
            yield return null;
        }

        //StartRound();
    }


    //clearing the level
    private void RemovePlayer(PlayerController pPlayer)
    {
        StartCoroutine(SpawnPS(pPlayer));
    }

    private IEnumerator SpawnPS(PlayerController pPlayer)
    {
        yield return new WaitForSeconds(2);
        if (pPlayer != null)
        {
            pPlayer.Hookable.Push(Vector3.down, 1);
            var ps = Instantiate(DestroyParticleEffect, pPlayer.transform.position, Quaternion.identity);
            Destroy(pPlayer.gameObject);
            Destroy(ps, 3f);
        }
    }

    private IEnumerator IEKillPlayer(PlayerController pPlayer, GameObject pParticleEffect = null, bool destroyOnEnd = false, bool pAttachToPlayer = false)
    {
        Players.Remove(pPlayer);
        if (!destroyOnEnd)
        {
            Destroy(pPlayer.gameObject);
        }
        else
        {
            SpawnedObjects.Add(pPlayer.gameObject);
        }

        if (pParticleEffect != null)
        {
            var ps = Instantiate(pParticleEffect, pPlayer.transform.position, Quaternion.identity);
            if (pAttachToPlayer)
            {
                ps.transform.SetParent(pPlayer.transform);
            }
            SpawnedObjects.Add(ps);
        }

        if (IsPlaying)
        {
            Difficulty++;
        }

        if (Players.Count == 1)
        {
            Camera.main.GetComponent<CameraFollow>().Zoom();
            
            var menuPlayer = PlayerDataModels.FirstOrDefault(a => a.PlayerID == Players.FirstOrDefault()?.PlayerID);
            
            if (menuPlayer != null)
            {
                RoundWinnerPlayerID = menuPlayer.PlayerID;
                if (WinParticles != null)
                {
                    var ps = Instantiate(WinParticles[menuPlayer.PlayerID - 1], Players[0].transform.position, Quaternion.identity);
                    //var pss = WinParticles.GetComponentsInChildren<ParticleSystem>();
                    //foreach (var item in pss)
                    //{ 
                    //    var main = item.main;
                    //    main.startColor = menuPlayer.PlayerColor;
                    //    main.playOnAwake = true;
                    //}
                    //ps.SetActive(true);
                    Destroy(ps, 3f);
                }
            }
            else
            {
                EndRound();
                yield break;
            }
            
            menuPlayer.RoundWins++;
            yield return new WaitForSeconds(1f);
           
            if (menuPlayer.RoundWins >= 3)
            {
                EndGame(menuPlayer);
                //StartGame();
            }
            else
            {
                EndRound();
            }
        }
    }
    //killing while game is ongoing
    public void KillPlayer(PlayerController pPlayer, GameObject pParticleEffect = null, bool destroyOnEnd = false, bool pAttachToPlayer = false)
    {
        StartCoroutine(IEKillPlayer(pPlayer, pParticleEffect, destroyOnEnd, pAttachToPlayer));
    }

    public PlayerDataModel AddPlayerDataModel(int pPlayerID, int pRewiredID, GameObject pPlayerPrefab)
    {
        PlayerDataModel playerInfo = new PlayerDataModel(pPlayerPrefab, pPlayerID, pRewiredID);

        if (!PlayerDataModels.Any(a => a.PlayerID == pPlayerID))
        {
            PlayerDataModels.Add(playerInfo);
            return playerInfo;
        }
        else
        {
            //print("This player id allready exists");
            return null;
        }
    }

    public void AddPlayerDataModel(PlayerDataModel pPlayerInfo)
    {
        if (!PlayerDataModels.Contains(pPlayerInfo))
        {
            PlayerDataModels.Add(pPlayerInfo);
        }
        else
        {
            //print("Player allready added to the gamemanager");
        }
    }
    /// <summary>
    /// Removes all spawned objects and players
    /// </summary>
    public void ClearArena()
    {
        SpawnedObjects.ForEach(a => StartCoroutine(RemoveItem(a)));
        SpawnedObjects.Clear();

        Players.ForEach(a => RemovePlayer(a));
        Players.Clear();
        StartCoroutine(TriggerPlayersCleared());

        HazardManager.Instance?.KillAllHazards();
    }

    private IEnumerator TriggerPlayersCleared()
    {
        yield return new WaitForSeconds(2.2f);
        if (FindObjectOfType<EventManager>() != null)
        {
            EventManager.TriggerEvent(EventManager.EventCodes.PlayersCleared);
            //Debug.Log("PlayersCleared event triggered");
            if (SpawnedObjects.Any()) // removes any(if any) powerups players throw right before dying
            {
                SpawnedObjects.ForEach(a => StartCoroutine(RemoveItem(a)));
                SpawnedObjects.Clear();
            }
        }
    }

    private IEnumerator RemoveItem(GameObject pObject)
    {
        yield return new WaitForSeconds(Random.Range(.2f, 1f));
        if (pObject != null)
        {
            Destroy(pObject);
            //var ps = Instantiate(DestroyParticleEffect, pObject.transform.position, Quaternion.identity);
            //Destroy(ps, 3f);
        }
    }

    public void SpawnPlayers()
    {
        var spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
        spawnPoints.Shuffle();
        for (int i = 0; i < PlayerDataModels.Count; i++)
        {
            Vector3 spawnPos = Vector3.up/2f;
            var spawnRot = Quaternion.identity;
            if (spawnPoints?.Length != 0 && spawnPoints[i] != null)
            {
                spawnPos = spawnPoints[i].transform.position;
                spawnRot = spawnPoints[i].transform.rotation;
            }

            StartCoroutine(SpawnPlayer(PlayerDataModels[i], spawnPos, spawnRot));
        }

        if (UseBots)
        {
            for (int i = 0; i < 4 - PlayerDataModels.Count; i++)
            {
                Vector3 spawnPos = Vector3.up / 2f;
                var spawnRot = Quaternion.identity;
                if (spawnPoints?.Length != 0 && spawnPoints[i + PlayerDataModels.Count] != null)
                {
                    spawnPos = spawnPoints[i + PlayerDataModels.Count].transform.position;
                    spawnRot = spawnPoints[i + PlayerDataModels.Count].transform.rotation;
                }

                StartCoroutine(SpawnPlayer(Bot, spawnPos, spawnRot));
            }
        }
    }

    private IEnumerator SpawnPlayer(PlayerDataModel pPlayerDataModel, Vector3 pSpawnPos, Quaternion pSpawnRot)
    {
        yield return new WaitForSeconds(/*Random.Range(.05f, .1f) +*/ pPlayerDataModel.PlayerID / 2.5f);
        var player = Instantiate(pPlayerDataModel.PlayerPrefab, pSpawnPos, pSpawnRot);
        var particleSystem = Instantiate(SpawnParticleEffect, player.transform.position, Quaternion.identity);
        
        ParticleSystem[] PS = particleSystem.GetComponentsInChildren<ParticleSystem>();
        for (int i = 0; i < PS.Length; i++)
        {
            ParticleSystem.MainModule main = PS[i].main;
            main.startColor = pPlayerDataModel.PlayerColor;
        }
        var playerController = player.GetComponent<PlayerController>();
        //playerController.ChangeState(PlayerController.State.none);
        playerController.SetUpPlayer(pPlayerDataModel.PlayerID, pPlayerDataModel.RewiredID, pPlayerDataModel.PlayerColor, pPlayerDataModel.Skin);
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.PlayerSound.Spawn);
        Players.Add(playerController);
        Destroy(particleSystem, 3f);
    }

    public void ChangeSkin(int pPlayerID, GameObject pSkinPrefab)
    {
        PlayerDataModels.FirstOrDefault(a => a.PlayerID == pPlayerID).PlayerPrefab = pSkinPrefab;
    }

    public void RemovePlayerDataModel(int pPlayerID)
    {
        PlayerDataModels.RemoveAll(a => a.PlayerID == pPlayerID);
    }

    public void ClearPlayerDataModels()
    {
        PlayerDataModels = new List<PlayerDataModel>();
    }

    public List<PlayerDataModel> GetPlayerDataModels()
    {
        return PlayerDataModels;
    }

    public void SetGamePaused(bool pValue)
    {
        if (GamePaused != pValue)
        {
            
            UIManager.Instance.SetPauseMenuAsActive(pValue);
            //ReInput.players.GetPlayer(pRewiredID).controllers.maps.SetMapsEnabled(pValue, "Menus");
            foreach (var player in ReInput.players.AllPlayers)
            {
                player.controllers.maps.SetMapsEnabled(pValue, "Menus");
            }

            foreach (var player in ReInput.players.AllPlayers)
            {
                player.controllers.maps.SetMapsEnabled(!pValue, "Gameplay");
            }
            Time.timeScale = System.Convert.ToInt32(!pValue);
            GamePaused = pValue;
            AudioManager.Instance.PauseSounds(pValue);
        }
    }

    public AudioMixer GetAudioMixer()
    {
        return Mixer;
    }

    public void LoadScreenAndLoadScene(int pSceneIndex, int pMenuScreenIndex = 0)
    {
        if (!LoadingScene)
        {
            LoadingScene = true;
            IsPlaying = false;
            var mainCanvas = GameObject.FindGameObjectWithTag("MainCanvas");
            var loadScreen = Instantiate(UIManager.Instance.LoadingScreenPrefab, mainCanvas.transform);
            loadScreen.SetActive(true);

            foreach (var player in ReInput.players.AllPlayers)
            {
                player.controllers.maps.SetAllMapsEnabled(false);
            }

            if (Players.Any())
            {
                Players.Clear();
            }
            StopAllCoroutines();
            StartCoroutine(LoadScene(pSceneIndex, pMenuScreenIndex));
        }
    }

    private IEnumerator LoadScene(int pSceneIndex, int pMenuScreenIndex)
    {
        //if (pSceneIndex == 0)
        //{
        //    asyncLoadLevel = SceneManager.LoadSceneAsync(pSceneIndex, LoadSceneMode.Additive);
        //}
        //else
        //{
        //    asyncLoadLevel = SceneManager.LoadSceneAsync(pSceneIndex);
        //}
        //asyncLoadLevel = SceneManager.LoadSceneAsync(pSceneIndex, LoadSceneMode.Additive);
        yield return new WaitForSecondsRealtime(2f);
        AsyncOperation asyncLoadLevel = SceneManager.LoadSceneAsync(pSceneIndex);

        while (!asyncLoadLevel.isDone)
        {
            //if (asyncLoadLevel.isDone)
            //{
            //    break;
            //}
            //yield return new WaitForEndOfFrame();
            yield return null;
        }

        if (pSceneIndex == 0)
        {
            FindObjectOfType<MenusManager>().GoToMenuWithoutTransition(pMenuScreenIndex);
            if (pMenuScreenIndex != 4 && pMenuScreenIndex != 3)
            {
                ClearPlayerDataModels();
            }
        }

        //yield return new WaitForSeconds(2f);
        //LoadingCoroutine = null;
        AudioManager.Instance.ChangingScenes();
        LoadingScene = false;
    }

    public void ChangeVolume(string pChannelName, float pSliderValue)
    {
        PlayerPrefs.SetFloat(pChannelName, pSliderValue);
        if (pSliderValue > 0)
        {
            Mixer.SetFloat(pChannelName, Mathf.Log10(pSliderValue) * 20);
        }
        else
        {
            Mixer.SetFloat(pChannelName, -80);
        }
    }
}
