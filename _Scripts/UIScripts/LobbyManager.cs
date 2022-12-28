using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Rewired;
using TMPro;

public class LobbyManager : MonoBehaviour {
    public GameObject PlayerPrefab;
    public GameObject BotAIPrefab;
    public GameObject BotButtonPrompts;
    //public GameObject ConfirmSprite;
    //public Image ConfirmFill;
    //public GameObject CountDownTimer;
    public List<Image> CharacterSprites;
    //public List<TextMeshProUGUI> PlayerNameTexts;
    public List<GameObject> JoinSprites;
    public List<GameObject> PlayerSprites;
    public List<GameObject> ReadyPlayerSprites;
    public List<GameObject> UnreadyPlayerSprites;
    public List<GameObject> BotDifficultySprites;
    public List<SkinModel> Skins;
    private MenusManager MenuManager;
    private List<float> Timers = new List<float>();
    [SerializeField]
    private List<int> ReadyPlayers = new List<int>();
    [HideInInspector]
    private bool Tournament;
    //private Coroutine CountdownCoroutine;
    [SerializeField]
    public Color StandardArrowColor;
    [SerializeField]
    public Color HighlightArrowColor;
    [Serializable]
    public class OutfitArrows
    {
        public Image Left;
        public Image Right;
    }
    public List<OutfitArrows> OutfitChangeArrowSprites;
    [Serializable]
    public class BotDifficultyArrows
    {
        public Image Up;
        public Image Down;
        public TextMeshProUGUI DifficultyText;
    }
    public List<BotDifficultyArrows> BotDifficultyArrowSprites;
    [SerializeField]
    private GameObject ContinueSprite;

    private Coroutine HoldToLeaveCoroutine;
    private bool CanContinue = false;
    private bool InputsActive = false;
    private float Timer;
    private float HoldButtonTimer = 0;
    private float TimeToHoldDown = 2;
    private List<int> PlayersHoldingButtonDown = new List<int>();
    [SerializeField]
    private GameObject BackPromptGO;
    [SerializeField]
    private Image BackPromptFill;
    

    private void Start ()
    {
        for (int i = 0; i < 4; i++)
        {
            Timers.Add(0f);
        }
        Timer = 0;

        MenuManager = MenusManager.Instance;
        //ResetSprites();
	}

    private void OnEnable()
    {
        StartCoroutine(ActivateInputs());
        ResetSprites();
        //CanGoBack = true;
        //GameManager.Instance.ClearPlayerDataModels();
        HoldToLeaveCoroutine = null;
        PlayersHoldingButtonDown.Clear();
        SetBackPromptPressed(false);
        ReadyPlayers.Clear();
        if (GameManager.Instance.PlayerDataModels.Any())
        {
            //var players = new List<PlayerDataModel>(GameManager.Instance.PlayerDataModels);
            //foreach (var player in players)
            //{
            //    int index = Skins.FindIndex(a => a.Skin == player.Skin);
            //    GameManager.Instance.RemovePlayerDataModel(player.PlayerID);
            //    AddPlayer(player.PlayerID, player.RewiredID);
            //    var newPlayer = GameManager.Instance.PlayerDataModels.FirstOrDefault(a => a.PlayerID == player.PlayerID);
            //    SetSkin(newPlayer, index);
            //}

            while (GameManager.Instance.PlayerDataModels.Where(a => a.RewiredID == (5)).ToList().Any())
            {
                RemoveBot();
            }

            foreach (var player in GameManager.Instance.PlayerDataModels)
            {
                UpdateLobbySprites(true, player.PlayerID, player.RewiredID);
                SetSkin(player, Skins.FindIndex(a => a.Skin == player.Skin));
                ReInput.players.GetPlayer(player.RewiredID).controllers.maps.SetMapsEnabled(true, "LobbyJoined");
            }
        }
        else
        {
            for (int i = 0; i < ReInput.players.playerCount; i++)
            {
                ReInput.players.GetPlayer(i).controllers.maps.SetMapsEnabled(true, "LobbyOut");
            }
        }
    }

    private void Update ()
    {
        if (InputsActive)
        {
            for (int i = 0; i < ReInput.players.playerCount; i++)
            {
                //if (!GameManager.Instance.PlayerDataModels.Any() && ReInput.players.GetPlayer(i).GetButtonDown("MenuBack"))
                //{
                //    LeaveLobbyScreen();
                //}

                var reInputPlayer = ReInput.players.GetPlayer(i);
                var playerDM = GameManager.Instance.PlayerDataModels.FirstOrDefault(a => a.RewiredID == i);

                if (playerDM != null && reInputPlayer.GetButtonShortPress("MenuBack") || playerDM == null && reInputPlayer.GetButton("MenuBack"))
                {
                    if (!PlayersHoldingButtonDown.Contains(i))
                    {
                        PlayersHoldingButtonDown.Add(i);
                    }

                    if (HoldToLeaveCoroutine == null)
                    {
                        HoldToLeaveCoroutine = StartCoroutine(HoldToLeave());
                    }
                }
                else if (reInputPlayer.GetButtonShortPressUp("MenuBack") || reInputPlayer.GetButtonUp("MenuBack"))
                {
                    PlayersHoldingButtonDown.Remove(i);
                }
            }

            //******has to be below getbutton(backbutton)
            GetLobbyInputs();

            //******has to be below getbutton(start game)
            GetJoinButtonsDown();

            GetBotAddOrRemoveInputs();
        }
    }

    private void SetBackPromptPressed(bool pValue)
    {
        Animator anim = BackPromptGO.GetComponent<Animator>();
        BackPromptFill.fillAmount = 0;

        anim.SetBool("Active", pValue);
    }

    private IEnumerator HoldToLeave()
    {
        HoldButtonTimer = 0;
        SetBackPromptPressed(true);
        while (PlayersHoldingButtonDown.Any())
        {
            HoldButtonTimer += Time.deltaTime * PlayersHoldingButtonDown.Count;
            BackPromptFill.fillAmount = HoldButtonTimer / TimeToHoldDown;

            yield return null;

            if (HoldButtonTimer >= TimeToHoldDown)
            {
                LeaveLobbyScreen();
                yield break;
            }
        }

        HoldToLeaveCoroutine = null;
        SetBackPromptPressed(false);
        yield break;
    }

    private IEnumerator ActivateInputs()
    {
        yield return new WaitForSeconds(0.8f);

        InputsActive = true;
    }

    private void GetJoinButtonsDown()
    {
        for (int i = 0; i < ReInput.players.playerCount; i++)
        {
            if (ReInput.players.GetPlayer(i).GetButtonDown("JoinLobby"))
            {
                if (GameManager.Instance.PlayerDataModels.Count < 4)
                {
                    OnJoinButtonDown(i);
                }
                else
                {
                    //Debug.Log("Lobby is full");
                }
            }
        }
    }

    private void LeaveLobbyScreen()
    {
        GameManager.Instance.ClearPlayerDataModels();
        ResetSprites();
        for (int i = 0; i < ReInput.players.playerCount; i++)
        {
            var player = ReInput.players.GetPlayer(i);
            player.controllers.maps.SetAllMapsEnabled(false);
            player.controllers.maps.SetMapsEnabled(true, "Menus");
        }
        SetBackPromptPressed(false);
        MenuManager.GoToMenu(MenuScreens.MainMenu);
    }

    private void ResetSprites()
    {
        for (int i = 0; i < 4; i++)
        {
            PlayerSprites[i].SetActive(false);
            JoinSprites[i].SetActive(true);
            ReadyPlayerSprites[i].SetActive(false);
            UnreadyPlayerSprites[i].SetActive(true);
            BotDifficultySprites[i].SetActive(false);
        }
    }

    private void GetBotAddOrRemoveInputs()
    {
        var playerDModels = GameManager.Instance.PlayerDataModels;
        if (playerDModels.Any())
        {
            var rewiredPlayer = ReInput.players.GetPlayer(playerDModels[0].RewiredID);
            Timer += +Time.deltaTime;

            if (rewiredPlayer.GetButtonDown("AddBot"))
            {
                if (GameManager.Instance.PlayerDataModels.Count < 4)
                {
                    AddBot();
                }
                else
                {
                    //Debug.Log("Lobby is full");
                }
            }

            if (rewiredPlayer.GetButtonDown("RemoveBot"))
            {
                if (GameManager.Instance.PlayerDataModels.Count != 1)
                {
                    RemoveBot();
                }
                else
                {
                    //Debug.Log("All Bots are gone");
                }
                
            }

            var BotDifficulty = rewiredPlayer.GetAxisRaw("BotDifficulty");
            if (/*ReadyPlayers.Contains(playerDModels[0].PlayerID) &&*/ playerDModels.Where(a => a.RewiredID == 5).Any())
            {
                if (Mathf.Abs(BotDifficulty) >= 0.5f && Timer > .3f)
                {
                    ChangeBotDifficulty(Math.Sign(BotDifficulty));
                    Timer = 0;
                }
            }
        }
    }

    private void GetLobbyInputs()
    {
        var playerDModels = GameManager.Instance.PlayerDataModels;
        if (playerDModels.Any())
        {
            for (int i = 0; i < playerDModels.Count; i++)
            {
                var rewiredPlayer = ReInput.players.GetPlayer(playerDModels[i].RewiredID);
                var playerID = playerDModels[i].PlayerID;
                Timers[i] += Time.deltaTime;

                if (CanContinue && rewiredPlayer.GetButtonDown("ConfirmLobby"))
                {
                    OnLobbyConfirmed();
                }

                else if (rewiredPlayer.GetButtonDown("Ready"))
                {
                    if (!ReadyPlayers.Contains(playerID))
                    {
                        SetPlayerReady(true, playerID);
                    }
                }

                else if (rewiredPlayer.GetButtonDown("ExitLobby"))
                {
                    if (ReadyPlayers.Contains(playerID))
                    {
                        SetPlayerReady(false, playerID);
                    }
                    else
                    {
                        RemovePlayer(playerID);
                        return;
                    }
                }

               

                var ChangeSkinAxis = rewiredPlayer.GetAxisRaw("ChangeSkin");
                if (Mathf.Abs(ChangeSkinAxis) >= 0.5f && Timers[i] > .3f && !ReadyPlayers.Contains(playerID))
                {
                    ChangeSkin(playerDModels[i], Math.Sign(ChangeSkinAxis));
                    AudioManager.Instance.PlayOneShot(AudioManager.Instance.UISound.ChangeOutfit);
                    Timers[i] = 0f;
                }
            }
        }
    }

    private void SetPlayerReady(bool pValue, int pPlayerID)
    {
        if (pValue)
        {
            ReadyPlayers.Add(pPlayerID);
            AudioManager.Instance.PlayOneShot(AudioManager.Instance.UISound.LobbyReady);
        }
        else
        {
            ReadyPlayers.Remove(pPlayerID);
        }

        ReadyPlayerSprites[pPlayerID - 1].SetActive(pValue);
        UnreadyPlayerSprites[pPlayerID - 1].SetActive(!pValue);

        if (ReadyPlayers.Count == GameManager.Instance.PlayerDataModels.Count && GameManager.Instance.PlayerDataModels.Count >= 2)
        {
            CanContinue = true;
            ContinueSprite.SetActive(true);
        }
        else
        {
            CanContinue = false;
            ContinueSprite.SetActive(false);
        }

    }

    private void ChangeBotDifficulty(int dir, bool manualSet = false)
    {
        var dataModels = GameManager.Instance.PlayerDataModels;

        // Only first player in lobby controls bot add/remove and difficulty
        bool up = dir > 0;
        var temp = GameManager.Instance.BotDifficulty;
        // change difficulty(0 = Hard / 1 = Medium / 2 = Easy)
        if (up)
        {
            GameManager.Instance.BotDifficulty--;
        }
        else
        {
            GameManager.Instance.BotDifficulty++;
        }

        if (GameManager.Instance.BotDifficulty <= 0)
        {
            GameManager.Instance.BotDifficulty = 0;
        }
        else if (GameManager.Instance.BotDifficulty >= 2)
        {
            GameManager.Instance.BotDifficulty = 2;
        }

        if (manualSet)
        {
            GameManager.Instance.BotDifficulty = dir;
        }

        if (GameManager.Instance.BotDifficulty != temp) //if difficulty changed, play sound and update ui elements
        {
            AudioManager.Instance.PlayOneShot(AudioManager.Instance.UISound.ChangeOutfit);

            var text = new List<TextMeshProUGUI>();
            // Highlight Arrows of all bots of input direction
            var arrows = new List<Image>();

            if (up)
            {
                for (int i = 0; i < dataModels.Count; i++)
                {
                    if (dataModels[i].RewiredID == 5)
                    {
                        arrows.Add(BotDifficultyArrowSprites[i].Up);
                        text.Add(BotDifficultyArrowSprites[i].DifficultyText);
                    }
                }
            }
            else
            {
                for (int i = 0; i < dataModels.Count; i++)
                {
                    if (dataModels[i].RewiredID == 5)
                    {
                        arrows.Add(BotDifficultyArrowSprites[i].Down);
                        text.Add(BotDifficultyArrowSprites[i].DifficultyText);
                    }
                }
            }
            for (int i = 0; i < arrows.Count; i++)
            {
                StartCoroutine(HighlightArrowsOnInput(arrows[i]));
            }

            // Update difficulty text
            for (int i = 0; i < BotDifficultyArrowSprites.Count; i++)
            {
                switch (GameManager.Instance.BotDifficulty)
                {

                    case 0:
                        BotDifficultyArrowSprites[i].DifficultyText.text = "Hard";
                        break;
                    case 1:
                        BotDifficultyArrowSprites[i].DifficultyText.text = "Normal";
                        break;
                    case 2:
                        BotDifficultyArrowSprites[i].DifficultyText.text = "Easy";
                        break;
                }
            }
        }
    }

    private void ChangeSkin(PlayerDataModel pMenuPlayer, int dir)
    {
        int index = Skins.FindIndex(a => a.Skin == pMenuPlayer.Skin) + dir;
        if (index < 0)
        {
            index = Skins.Count - 1;
        }
        else if (index >= Skins.Count)
        {
            index = 0;
        }

        Image highlightArrow;
        if (dir < 0)
        {
            highlightArrow = OutfitChangeArrowSprites[pMenuPlayer.PlayerID - 1].Left;
        }
        else
        {
            highlightArrow = OutfitChangeArrowSprites[pMenuPlayer.PlayerID - 1].Right;
        }

        StartCoroutine(HighlightArrowsOnInput(highlightArrow));
        SetSkin(pMenuPlayer, index);
        //send help
        
    }

    private IEnumerator HighlightArrowsOnInput(Image pArrowToChange)
    {
        pArrowToChange.color = HighlightArrowColor;
        yield return new WaitForSeconds(0.1f);
        pArrowToChange.color = StandardArrowColor;
    }

    private void SetSkin(PlayerDataModel pPlayerModel, int pIndex)
    {
        pPlayerModel.Skin = Skins[pIndex].Skin;
        pPlayerModel.PlayerSprite = Skins[pIndex].GetSprite(pPlayerModel.PlayerID);
        CharacterSprites[pPlayerModel.PlayerID - 1].sprite = Skins[pIndex].GetSprite(pPlayerModel.PlayerID);
        //SetName(pPlayerModel, Skins[pIndex]);
    }

    //private void SetName(PlayerDataModel pPlayerModel, SkinModel pSkinModel)
    //{
    //    string name = "";

    //    switch (pPlayerModel.PlayerID)
    //    {
    //        case 1:
    //            name = "Green";
    //            break;
    //        case 2:
    //            name = "Red";
    //            break;
    //        case 3:
    //            name = "Blue";
    //            break;
    //        case 4:
    //            name = "Purple";
    //            break;
    //    }

    //    name += " " + pSkinModel.SkinName + " shark";
    //    pPlayerModel.PlayerName = name;
    //    //PlayerNameTexts[pPlayerModel.PlayerID - 1].text = name;
    //}

    private void OnJoinButtonDown(int pRewiredID)
    {
        // int playerID = GameManager.GetPlayerDataModels()
        int playerid = GetPlayerID();

        CanContinue = false;
        ContinueSprite.SetActive(false);
        BotButtonPrompts.SetActive(true);
        AddPlayer(playerid, pRewiredID);
    }

    private int GetPlayerID(int pID = 1)
    {
        if (GameManager.Instance.PlayerDataModels.Any(a => a.PlayerID == pID))
        {
            return GetPlayerID(pID + 1);
        }
        else
        {
            return pID;
        }
    }

    private void AddPlayer(int pPlayerID, int pRewiredID)
    {
        var PlayerDM = GameManager.Instance.AddPlayerDataModel(pPlayerID, pRewiredID, PlayerPrefab);
        SetSkin(PlayerDM, 0);

        //menuplayer.Skin = Skins[0].Skin;
        //menuplayer.PlayerSprite = Skins[0].GetSprite(pPlayerID);
        //CharacterSprites[pPlayerID - 1].sprite = Skins[0].GetSprite(pPlayerID);
        UpdateLobbySprites(true, pPlayerID, PlayerDM.RewiredID);
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.UISound.ButtonClick);

        var rewiredPlayerMaps = ReInput.players.GetPlayer(pRewiredID).controllers.maps;

        rewiredPlayerMaps.SetMapsEnabled(true, "LobbyJoined");
        rewiredPlayerMaps.SetMapsEnabled(false, "LobbyOut");
        //rewiredPlayerMaps.SetMapsEnabled(false, "Menus");
    }

    private void AddBot()
    {
        if (!GameManager.Instance.PlayerDataModels.Where(a => a.RewiredID == 5).Any())
        {
            ChangeBotDifficulty(1, true);
        }

        int playerid = GetPlayerID();
        var PlayerDM = GameManager.Instance.AddPlayerDataModel(playerid, 5, BotAIPrefab);

        var availableSkins = Skins.ToList();
        List<SkinModel> usedSkins = new List<SkinModel>();

        foreach (var dm in GameManager.Instance.PlayerDataModels)
        {
            if (dm != null)
            {
                usedSkins.Add(Skins.FirstOrDefault(a => a.Skin == dm.Skin));
            }
        }
        if (usedSkins.Any())
        {
            foreach (var item in usedSkins)
            {
                availableSkins.Remove(item);
            }
        }
        var rng = UnityEngine.Random.Range(0, availableSkins.Count);
        var skin = availableSkins[rng];
        int skinIndex = Skins.IndexOf(skin);

        SetSkin(PlayerDM, skinIndex);
        UpdateLobbySprites(true, playerid, PlayerDM.RewiredID);
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.UISound.ButtonClick);

        SetPlayerReady(true, playerid);
        BotDifficultySprites[playerid-1].SetActive(true);
    }

    private void RemovePlayer(int pPlayerID)
    {
        int rewiredID = GameManager.Instance.PlayerDataModels.FirstOrDefault(a => a.PlayerID == pPlayerID).RewiredID;

        AudioManager.Instance.PlayOneShot(AudioManager.Instance.UISound.LobbyPlayerLeave);

        //ReInput.players.GetPlayer(rewiredID).controllers.maps.SetMapsEnabled(false, "LobbyJoined");
        //ReInput.players.GetPlayer(rewiredID).controllers.maps.SetMapsEnabled(true, "LobbyOut");
        //ReInput.players.GetPlayer(rewiredID).controllers.maps.SetMapsEnabled(true, "Menus");

        var rewiredPlayerMaps = ReInput.players.GetPlayer(rewiredID).controllers.maps;

        rewiredPlayerMaps.SetMapsEnabled(false, "LobbyJoined");
        rewiredPlayerMaps.SetMapsEnabled(true, "LobbyOut");
        rewiredPlayerMaps.SetMapsEnabled(true, "Menus");

        GameManager.Instance.RemovePlayerDataModel(pPlayerID);
        UpdateLobbySprites(false, pPlayerID, rewiredID);

        if (!GameManager.Instance.PlayerDataModels.Where(a => a.RewiredID != (5)).ToList().Any())
        {
            //for (int i = 0; i < GameManager.Instance.PlayerDataModels.Where(a => a.RewiredID == (-1)).ToList().Count; i++)
            //{
            //    RemoveBot();
            //}
            BotButtonPrompts.SetActive(false);
            while (GameManager.Instance.PlayerDataModels.Where(a => a.RewiredID == (5)).ToList().Any())
            {
                RemoveBot();
            }
        }

        if (ReadyPlayers.Count == GameManager.Instance.PlayerDataModels.Count && GameManager.Instance.PlayerDataModels.Count >= 2)
        {
            CanContinue = true;
            ContinueSprite.SetActive(true);
        }
    }

    private void RemoveBot()
    {
        var bot = GameManager.Instance.PlayerDataModels.LastOrDefault(a => a.RewiredID == 5);

        AudioManager.Instance.PlayOneShot(AudioManager.Instance.UISound.LobbyPlayerLeave);

        GameManager.Instance.RemovePlayerDataModel(bot.PlayerID);
        SetPlayerReady(false, bot.PlayerID);
        UpdateLobbySprites(false, bot.PlayerID, bot.RewiredID);
    }

    private void UpdateLobbySprites(bool pJoined, int pPlayerID, int pRewiredID)
    {
        //MenuManager.PlayChangedOptionEffect();
        int arrayIndex = pPlayerID - 1;
        JoinSprites[arrayIndex].SetActive(!pJoined);
        PlayerSprites[arrayIndex].SetActive(pJoined);
        ReadyPlayerSprites[arrayIndex].SetActive(false);
        if (pRewiredID == 5)
        {
            BotDifficultySprites[arrayIndex].SetActive(pJoined);
        }
    }

    //private void OnDisable()
    //{
    //    ConfirmSprite.SetActive(false);
    //}

    private void OnLobbyConfirmed()
    {
        MenuManager.GoToMenu(MenuScreens.MapAndModes);
        ContinueSprite.SetActive(false);
        CanContinue = false;
        //HoldStart = 0;
        //ConfirmSprite.SetActive(false);
        for (int i = 0; i < ReInput.players.playerCount; i++)
        {
            var player = ReInput.players.GetPlayer(i);
            player.controllers.maps.SetMapsEnabled(false, "LobbyJoined");
            player.controllers.maps.SetMapsEnabled(true, "Menus");
        }
    }

    //private IEnumerator CountDown()
    //{
    //    float timer = 0;
    //    while (timer < 3)
    //    {
    //        timer += Time.deltaTime;
    //        yield return null;
    //    }

    //    CountDownTimer.SetActive(false);
    //    OnLobbyConfirmed(); 
    //}

    private void OnDisable()
    {
        InputsActive = false;
    }
}
