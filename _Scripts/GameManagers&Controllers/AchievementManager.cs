using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using System;
using System.Linq;

public class AchievementManager : MonoBehaviour
{
    private enum Achievement : int
    {
        achievement_00,
        achievement_01,
        achievement_02,
        achievement_03,
        achievement_04,
        achievement_05,
        achievement_06,
        achievement_07,
        achievement_08,
        achievement_09,
        achievement_10,
        achievement_11,
        achievement_12,
        achievement_13,
        achievement_14,
        achievement_15,
        achievement_16,
        achievement_17,
        achievement_18,
        achievement_19,
        achievement_20,
        achievement_21,
        achievement_22,
        achievement_23,
        achievement_24,
        achievement_25,
        achievement_26,
        achievement_27,
        achievement_28
    };

    private Achievement_t[] Achievements = new Achievement_t[]
    {
        new Achievement_t(Achievement.achievement_00, "Chum", "Win a match against 3 bots on normal difficulty or harder without losing a round"),
        new Achievement_t(Achievement.achievement_01, "Thresher", "Win a match against 3 bots on hard difficulty without losing a round"),
        new Achievement_t(Achievement.achievement_02, "Baby Shark", "Win a tournament against 3 bots on normal difficulty or harder"),
        new Achievement_t(Achievement.achievement_03, "Hammerhead Shark", "Win a tournament against 3 bots on hard difficulty"),
        new Achievement_t(Achievement.achievement_04, "Tiger Shark", "Win a super tournament against 3 bots on normal difficulty"),
        new Achievement_t(Achievement.achievement_05, "Great White", "Win a super tournament against 3 bots on hard difficulty"),
        new Achievement_t(Achievement.achievement_06, "Megalodon", "Win a tournament against bots (without them getting any trophies)"),
        new Achievement_t(Achievement.achievement_07, "Drop the anchor!", "Hit an opponent 50 times with an anchor"),
        new Achievement_t(Achievement.achievement_08, "Captain Ahab", "Hit an opponent 50 times with a harpoon"),
        new Achievement_t(Achievement.achievement_09, "Bubble Trouble", "Hit an opponent 50 times with a bubble"),
        new Achievement_t(Achievement.achievement_10, "Mastered the unpredictable!", "Hit an opponent 50 times with a boomerang"),
        new Achievement_t(Achievement.achievement_11, "Dyn-o-mite!", "Hit an opponent 50 times with a dynamite"),
        new Achievement_t(Achievement.achievement_12, "SUCTION!", "Hit an opponent 50 times with an implosion orb"),
        new Achievement_t(Achievement.achievement_13, "Shark’s Discus", "Hit an opponent 50 times with a disk"),
        new Achievement_t(Achievement.achievement_14, "Sharkwatch", "Win a match on the Beach level as a lifeguard shark"),
        new Achievement_t(Achievement.achievement_15, "I am the captain now", "Win a match on the Pirate ship level as a pirate shark"),
        new Achievement_t(Achievement.achievement_16, "King of the Castle", "Win a match on the Castle level as a knight shark"),
        new Achievement_t(Achievement.achievement_17, "Floor is lava", "Win a match on the Volcano level as a tiki shark"),
        new Achievement_t(Achievement.achievement_18, "Houston we have a winner", "Win a match on the Space station level as an astronaut shark"),
        new Achievement_t(Achievement.achievement_19, "Fortune and glory", "Win a match on the Temple level as an archeologist shark"),
        new Achievement_t(Achievement.achievement_20, "Armed to the teeth", "Pick up every powerup"),
        new Achievement_t(Achievement.achievement_21, "Sleeping with the fishes", "Get hit by every hazard"),
        new Achievement_t(Achievement.achievement_22, "Adios stupido", "Get hit by a boulder in the Temple level as the archeologist shark"),
        new Achievement_t(Achievement.achievement_23, "Tis but a scratch", "Get hit by the barrage in the Castle level as the knight shark"),
        new Achievement_t(Achievement.achievement_24, "You had one job", "Get hit by a wave in the Beach level as the lifeguard shark"),
        new Achievement_t(Achievement.achievement_25, "Man overboard", "Get hit by a cannon in the Pirate ship level as the pirate shark"),
        new Achievement_t(Achievement.achievement_26, "Houston we have a problem", "Get sucked out of the Space station as the astronaut shark"),
        new Achievement_t(Achievement.achievement_27, "Home field disadvantage", "Be the first to get eliminated in the Volcano level as the tiki shark"),
        new Achievement_t(Achievement.achievement_28, "Fly fishing", "Using the fishing pole, hook an opponent that’s airborne")
    };


    protected static AchievementManager s_instance;
    public static AchievementManager Instance
    {
        get
        {
            if (s_instance == null)
            {
                return new GameObject("AchievementManager").AddComponent<AchievementManager>();
            }
            else
            {
                return s_instance;
            }
        }
    }

    private bool UnlockTest = false;
    private CGameID GameID;

    // Did we get stats from steam?
    private bool m_RequestedStats;
    private bool m_StatsValid;

    // Should we store stats this frame?
    private bool m_StoreStats;

    // Persisted Stat details
    // Usables Thrown
    private int s_HarpoonHits;
    private int s_AnchorHits;
    private int s_DiscHits;
    private int s_BubbleHits;
    private int s_BoomerangHits;
    private int s_DynamiteHits;
    private int s_ImplosionHits;
    // Win AgainstBots
    private int s_AceAgainstBotsNormal;
    private int s_AceAgainstBotsHard;
    private int s_TournamentWinBotsNormal;
    private int s_TournamentWinBotsHard;
    private int s_SuperTournamentWinBotsNormal;
    private int s_SuperTournamentWinBotsHard;
    private int s_AceTournamentAgainstBots;
    // Win map as specific skin
    private int s_WinBeachLifeguard;
    private int s_WinPirateShipPirate;
    private int s_WinVolcanoTiki;
    private int s_WinCastleKnight;
    private int s_WinSpaceStationAstronaut;
    private int s_WinTempleArcheologist;

    // Pick up all powerups
    private int s_PickUpAnchor;
    private int s_PickUpShipHarpoon;
    private int s_PickUpSpaceHarpoon;
    private int s_PickUpShieldDisc;
    private int s_PickUpWheelDisc;
    private int s_PickUpSpaceDisc;
    private int s_PickUpBubble;
    private int s_PickUpBoomerang;
    private int s_PickupDynamite;
    private int s_PickUpTempleImplosion;
    private int s_PickUpSpaceImplosion;

    // Hit By Hazards
    private int s_HitByWave;
    private int s_HitByBeachBall;
    private int s_HitBySeaMine;
    private int s_HitByCannons;
    private int s_HitByBarrage;
    private int s_HitByBoulder;
    private int s_HitByPillarDart;
    private int s_HitByPressurePlate;
    private int s_HitByAirlock;
    private int s_HitByGrates;

    // Misc
    private int s_PickUpAllPowerups;
    private int s_GetHitByAllHazards;
    private int s_GetHitByBoulderAsArcheologist;
    private int s_GetHitByBarrageAsKnight;
    private int s_GetHitByWaveAsLifeguard;
    private int s_GetSuckedOutAsAstronaut;
    private int s_GetHitByCannonAsPirate;
    private int s_FirstInLavaAsTiki;
    private int s_HookSomeoneAirborne;

    protected Callback<UserStatsReceived_t> UserStatsRecieved;
    protected Callback<UserStatsStored_t> UserStatsStored;
    protected Callback<UserAchievementStored_t> UserAchievementStored;

    public bool DEBUG_MODE = false;

    private void Awake()
    {
        if (s_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        s_instance = this;
    }

    private void OnEnable()
    {
        if (!SteamManager.Initialized)
        {
            return;
        }

        // Cache the GameID for use in the Callbacks
        GameID = new CGameID(SteamUtils.GetAppID());

        UserStatsRecieved = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
        UserStatsStored = Callback<UserStatsStored_t>.Create(OnUserStatsStored);
        UserAchievementStored = Callback<UserAchievementStored_t>.Create(OnUserAchievementStored);

        // These need to be reset to get the stats upon an Assembly reload in the Editor.
        m_RequestedStats = false;
        m_StatsValid = false;

        // Register Achievement Listeners
        GameEndEvent.RegisterListener(OnGameEndEvent);
        UsableHitEvent.RegisterListener(OnUsableHitEvent);
        HazardEvent.RegisterListener(OnHazardEvent);
        PowerupPickupEvent.RegisterListener(OnPowerupPickupEvent);
    }

    private void OnPowerupPickupEvent(PowerupPickupEvent pInfo)
    {
        //Debug.LogFormat("<color=#008000ff>Powerup Pickup Event Fired </color>");
        if (pInfo.RewiredID != 5) // Not a bot
        {
            switch (pInfo.Usable)
            {
                case Usable.UsableType.Harpoon:
                    if (s_PickUpShipHarpoon == 0)
                    {
                        s_PickUpShipHarpoon = 1;
                        s_PickUpAllPowerups++;
                    }
                    break;
                case Usable.UsableType.SpaceHarpoon:
                    if (s_PickUpSpaceHarpoon == 0)
                    {
                        s_PickUpSpaceHarpoon = 1;
                        s_PickUpAllPowerups++;
                    }
                    break;
                case Usable.UsableType.Anchor:
                    if (s_PickUpAnchor == 0)
                    {
                        s_PickUpAnchor = 1;
                        s_PickUpAllPowerups++;
                    }
                    break;
                case Usable.UsableType.ShieldDisc:
                    if (s_PickUpShieldDisc == 0)
                    {
                        s_PickUpShieldDisc = 1;
                        s_PickUpAllPowerups++;
                    }
                    break;
                case Usable.UsableType.WheelDisc:
                    if (s_PickUpWheelDisc == 0)
                    {
                        s_PickUpWheelDisc = 1;
                        s_PickUpAllPowerups++;
                    }
                    break;
                case Usable.UsableType.SpaceDisc:
                    if (s_PickUpSpaceDisc == 0)
                    {
                        s_PickUpSpaceDisc= 1;
                        s_PickUpAllPowerups++;
                    }
                    break;
                case Usable.UsableType.Boomerang:
                    if (s_PickUpBoomerang == 0)
                    {
                        s_PickUpBoomerang = 1;
                        s_PickUpAllPowerups++;
                    }
                    break;
                case Usable.UsableType.Bubble:
                    if (s_PickUpBubble == 0)
                    {
                        s_PickUpBubble = 1;
                        s_PickUpAllPowerups++;
                    }
                    break;
                case Usable.UsableType.Dynamite:
                    if (s_PickupDynamite == 0)
                    {
                        s_PickupDynamite = 1;
                        s_PickUpAllPowerups++;
                    }
                    break;
                case Usable.UsableType.TempleImplosion:
                    if (s_PickUpTempleImplosion == 0)
                    {
                        s_PickUpTempleImplosion = 1;
                        s_PickUpAllPowerups++;
                    }
                    break;
                case Usable.UsableType.SpaceImplosion:
                    if (s_PickUpSpaceImplosion == 0)
                    {
                        s_PickUpSpaceImplosion = 1;
                        s_PickUpAllPowerups++;
                    }
                    break;
                default:
                    break;
            }
            m_StoreStats = true;
        }
    }

    private void OnHazardEvent(HazardEvent pInfo)
    {
        //Debug.LogFormat("<color=#008000ff>Hazard Event Fired </color>");
        if (pInfo.RewiredID != 5) // If not a bot
        {
            // check which hazard, mark which has been hit
            switch (pInfo.HazardType)
            {
                case AchHazardType.Wave:
                    if (s_HitByWave == 0)
                    {
                        s_HitByWave = 1;
                        s_GetHitByAllHazards++;
                    }
                    if (s_GetHitByWaveAsLifeguard == 0 && CheckIfCorrectSkin(pInfo.Skin.ToString(), Constants.SkinNames.Lifeguard))
                    {
                        s_GetHitByWaveAsLifeguard = 1;
                    }
                    break;
                case AchHazardType.SeaMine:
                    if (s_HitBySeaMine == 0)
                    {
                        s_HitBySeaMine = 1;
                        s_GetHitByAllHazards++;
                    }
                    break;
                case AchHazardType.BeachBall:
                    if (s_HitByBeachBall == 0)
                    {
                        s_HitByBeachBall = 1;
                        s_GetHitByAllHazards++;
                    }
                    break;
                case AchHazardType.Cannons:
                    if (s_HitByCannons == 0)
                    {
                        s_HitByCannons = 1;
                        s_GetHitByAllHazards++;
                    }

                    if (s_GetHitByCannonAsPirate == 0 && CheckIfCorrectSkin(pInfo.Skin.ToString(), Constants.SkinNames.Pirate))
                    {
                        s_GetHitByCannonAsPirate = 1;
                    }
                    break;
                case AchHazardType.Grater:
                    if (s_HitByGrates == 0)
                    {
                        s_HitByGrates = 1;
                        s_GetHitByAllHazards++;
                    }
                    break;
                case AchHazardType.Barrage:
                    if (s_HitByBarrage == 0)
                    {
                        s_HitByBarrage = 1;
                        s_GetHitByAllHazards++;
                    }
                    if (s_GetHitByBarrageAsKnight == 0 && CheckIfCorrectSkin(pInfo.Skin.ToString(), Constants.SkinNames.Knight))
                    {
                        s_GetHitByBarrageAsKnight = 1;
                    }
                    break;
                case AchHazardType.Airlock:
                    if (s_HitByAirlock == 0)
                    {
                        s_HitByAirlock = 1;
                        s_GetHitByAllHazards++;
                    }

                    if (s_GetSuckedOutAsAstronaut == 0 && CheckIfCorrectSkin(pInfo.Skin.ToString(), Constants.SkinNames.Astronaut))
                    {
                        s_GetSuckedOutAsAstronaut = 1;
                    }
                    break;
                case AchHazardType.Boulder:
                    if (s_HitByBoulder == 0)
                    {
                        s_HitByBoulder = 1;
                        s_GetHitByAllHazards++;
                    }

                    if (s_GetHitByBoulderAsArcheologist == 0 && CheckIfCorrectSkin(pInfo.Skin.ToString(), Constants.SkinNames.Archaeologist))
                    {
                        s_GetHitByBoulderAsArcheologist = 1;
                    }
                    break;
                case AchHazardType.Pillars:
                    if (s_HitByPillarDart == 0)
                    {
                        s_HitByPillarDart = 1;
                        s_GetHitByAllHazards++;
                    }
                    break;
                case AchHazardType.Pressureplate:
                    if (s_HitByPressurePlate == 0)
                    {
                        s_HitByPressurePlate = 1;
                        s_GetHitByAllHazards++;
                    }
                    break;
                case AchHazardType.Lava:
                    if (s_FirstInLavaAsTiki == 0 && pInfo.HitCount == 1 && CheckIfCorrectSkin(pInfo.Skin.ToString(), Constants.SkinNames.Tiki))
                    {
                        s_FirstInLavaAsTiki = 1;
                    }
                    break;
                case AchHazardType.None:
                    break;
                default:
                    break;
            }
            m_StoreStats = true;
        }
    }

    private void OnUsableHitEvent(UsableHitEvent pInfo)
    {
        //Debug.LogFormat("<color=#008000ff> Usable Hit Event Fired </color>");
        if (pInfo.RewiredID != 5 && pInfo.PlayerHit != pInfo.UsableOwner) // If not a bot and not hitting yourself
        {
            switch (pInfo.Usable)
            {
                case Usable.UsableType.StandardHook:
                    if (s_HookSomeoneAirborne == 0 && pInfo.PlayerHit.transform.position.y >= 3f)
                    {
                        s_HookSomeoneAirborne = 1;
                    }
                    break;
                case Usable.UsableType.Harpoon:
                    if (s_HarpoonHits <= 50)
                    {
                        s_HarpoonHits++;
                    }
                    break;
                case Usable.UsableType.SpaceHarpoon:
                    if (s_HarpoonHits <= 50)
                    {
                        s_HarpoonHits++;
                    }
                    break;
                case Usable.UsableType.Anchor:
                    if (s_AnchorHits <= 50)
                    {
                        s_AnchorHits++;
                    }
                    break;
                case Usable.UsableType.ShieldDisc:
                    if (s_DiscHits <= 50)
                    {
                        s_DiscHits++;
                    }
                    break;
                case Usable.UsableType.WheelDisc:
                    if (s_DiscHits <= 50)
                    {
                        s_DiscHits++;
                    }
                    break;
                case Usable.UsableType.SpaceDisc:
                    if (s_DiscHits <= 50)
                    {
                        s_DiscHits++;
                    }
                    break;
                case Usable.UsableType.Boomerang:
                    if (s_BoomerangHits <= 50)
                    {
                        s_BoomerangHits++;
                    }
                    break;
                case Usable.UsableType.Bubble:
                    if (s_BubbleHits <= 50)
                    {
                        s_BubbleHits++;
                    }
                    break;
                case Usable.UsableType.Dynamite:
                    if (s_DynamiteHits <= 50)
                    {
                        s_DynamiteHits++;
                    }
                    break;
                case Usable.UsableType.TempleImplosion:
                    if (s_ImplosionHits <= 50)
                    {
                        s_ImplosionHits++;
                    }
                    break;
                case Usable.UsableType.SpaceImplosion:
                    if (s_ImplosionHits <= 50)
                    {
                        s_ImplosionHits++;
                    }
                    break;
                default:
                    break;
            }
            m_StoreStats = true;
        }
    }

    private void OnGameEndEvent(GameEndEvent pInfo)
    {
        if (pInfo.Winner.RewiredID != 5) // If not a bot won
        {
            bool onlyOnePlayer = pInfo.PlayerDataModels.Where(a => a != pInfo.Winner).ToList().All(a => a.RewiredID == 5);
            
            float botCount = pInfo.PlayerDataModels.Where(a => a != pInfo.Winner && a.RewiredID == 5).ToList().Count;

            if (onlyOnePlayer && botCount == 3)
            {
                // Checks if winner aced the Match/Tournament
                if (pInfo.TournamentOver)
                {
                    // If player didn't ace, still checks and unlocks the achievements "Win (super)Tournament against bots(+difficulty)"
                    CheckIfWonOrAcedTournament(pInfo);
                }
                
                CheckIfAcedGame(pInfo);
            }

            CheckWinAsSpecificSkin(pInfo);
            m_StoreStats = true;
        }
    }

    private void CheckWinAsSpecificSkin(GameEndEvent pInfo)
    {
        /* Map Index
         * 1 = Beach
         * 2 = PirateShip
         * 3 = Volcano
         * 4 = SpaceStation
         * 5 = Castle
         * 6 = Temple
         */
        switch (pInfo.MapIndex)
        {
            case 1:
                if (s_WinBeachLifeguard == 0 && CheckIfCorrectSkin(pInfo.Skin, Constants.SkinNames.Lifeguard))
                {
                    s_WinBeachLifeguard = 1;
                }
                break;
            case 2:
                if (s_WinPirateShipPirate == 0 && CheckIfCorrectSkin(pInfo.Skin, Constants.SkinNames.Pirate))
                {
                    s_WinPirateShipPirate = 1;
                }
                break;
            case 3:
                if (s_WinVolcanoTiki == 0 && CheckIfCorrectSkin(pInfo.Skin, Constants.SkinNames.Tiki))
                {
                    s_WinVolcanoTiki = 1;
                }
                break;
            case 4:
                if (s_WinSpaceStationAstronaut == 0 && CheckIfCorrectSkin(pInfo.Skin, Constants.SkinNames.Astronaut))
                {
                    s_WinSpaceStationAstronaut = 1;
                }
                break;
            case 5:
                if (s_WinCastleKnight == 0 && CheckIfCorrectSkin(pInfo.Skin, Constants.SkinNames.Knight))
                {
                    s_WinCastleKnight = 1;
                }
                break;
            case 6:
                if (s_WinTempleArcheologist == 0 && CheckIfCorrectSkin(pInfo.Skin, Constants.SkinNames.Archaeologist))
                {
                    s_WinTempleArcheologist = 1;
                }
                break;
            default:
                break;
        }
    }

    private bool CheckIfCorrectSkin(string pSkin, string pConstantSkinName)
    {
        return pSkin.Contains(pConstantSkinName);
    }

    private void CheckIfWonOrAcedTournament(GameEndEvent pInfo)
    {
        int sum = 0;
        pInfo.Contenders.Where(a => a.Score < pInfo.TournamentScoreNeeded).ToList().ForEach(a => sum += a.Score);
        // If sum of the scores excluding the winner. Sum being 0 means player won without anyone else getting a score
        if (sum == 0)
        {
            if (s_AceTournamentAgainstBots == 0)
            {
                s_AceTournamentAgainstBots = 1;
            }
        }

        if (pInfo.TournamentScoreNeeded == 3)
        {
            switch (pInfo.BotDifficulty)
            {
                case 0: // Hard
                    if (s_TournamentWinBotsHard == 0)
                    {
                        s_TournamentWinBotsHard = 1;
                        s_TournamentWinBotsNormal = 1;
                    }
                    break;
                case 1: // Normal
                    if (s_TournamentWinBotsNormal == 0)
                    {
                        s_TournamentWinBotsNormal = 1;
                    }
                    break;
                case 2: // Easy
                    
                    break;
                default:
                    break;
            }
        }

        if (pInfo.TournamentScoreNeeded == 6)
        {
            switch (pInfo.BotDifficulty)
            {
                case 0: // Hard
                    if (s_SuperTournamentWinBotsHard == 0)
                    {
                        s_SuperTournamentWinBotsHard = 1;
                        s_SuperTournamentWinBotsNormal = 1;
                    }
                    break;
                case 1: // Normal
                    if (s_SuperTournamentWinBotsNormal == 0)
                    {
                        s_SuperTournamentWinBotsNormal = 1;
                    }
                    break;
                case 2: // Easy
                    
                    break;
                default:
                    break;
            }
        }
    }

    private void CheckIfAcedGame(GameEndEvent pInfo)
    {
        int sum = 0;
        pInfo.PlayerDataModels.Where(a => a.RoundWins < 3).ToList().ForEach(a => sum += a.RoundWins);
       
        if (sum == 0)
        {
            switch (pInfo.BotDifficulty)
            {
                case 0: // Hard
                    if (s_AceAgainstBotsHard == 0)
                    {
                        s_AceAgainstBotsHard = 1;
                        s_AceAgainstBotsNormal = 1;
                    }
                    break;
                case 1: // Normal
                    if (s_AceAgainstBotsNormal == 0)
                    {
                        s_AceAgainstBotsNormal = 1;
                    }
                    break;
                case 2: // Easy
                    
                    break;
                default:
                    break;
            }
        }
    }

    private void Update()
    {
        if (!SteamManager.Initialized)
            return;

        if (!m_RequestedStats)
        {
            // is steam loaded?
            if (!SteamManager.Initialized)
            {
                m_RequestedStats = true;
                return;
            }

            // if yes, request our stats
            bool success = SteamUserStats.RequestCurrentStats();

            // This function should only return false if we weren't logged in, and we already checked that.
            // But handle it being false again anyway, just ask again later.
            m_RequestedStats = success;
        }

        if (DEBUG_MODE && Input.GetKeyDown(KeyCode.F9))
        {
            DEBUG_ResetAllStatsAndAchievements();
        }

        if (DEBUG_MODE && Input.GetKeyDown(KeyCode.F5))
        {
            DEBUG_KillAllBots();
        }

        if (DEBUG_MODE && Input.GetKeyDown(KeyCode.F8))
        {
            DEBUG_IncrementAllHitStats();
        }

        if (!m_StatsValid)
            return;

        // Evaluate achievements
        foreach (Achievement_t achievement in Achievements)
        {
            if (achievement.Achieved)
                continue;
            // Maybe only to evalute stat specific achievements, with if() statements inside the switch
            // for example if (won 10 games against bots) -> unlock that achievement or if (thrown 50 harpoons) -> unlock

            switch (achievement.AchievementID)
            {
                case Achievement.achievement_00:
                    //Win a match against 3 bots on normal difficulty or harder without losing a round.
                    if (s_AceAgainstBotsNormal == 1)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.achievement_01:
                    //Win a match against 3 bots on hard difficulty without losing a round.
                    if (s_AceAgainstBotsHard == 1)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.achievement_02:
                    //Win a tournament against 3 bots on normal difficulty or harder.
                    if (s_TournamentWinBotsNormal == 1)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.achievement_03:
                    //Win a tournament against 3 bots on hard difficulty.
                    if (s_TournamentWinBotsHard == 1)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.achievement_04:
                    //Win a super tournament against 3 bots on normal difficulty.
                    if (s_SuperTournamentWinBotsNormal == 1)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.achievement_05:
                    //Win a super tournament against 3 bots on hard difficulty.
                    if (s_SuperTournamentWinBotsHard == 1)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.achievement_06:
                    //Win a tournament against bots(without them getting any trophies).
                    if (s_AceTournamentAgainstBots == 1)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.achievement_07:
                    //Hit an opponent 50 times with an Anchor
                    if (s_AnchorHits >= 50)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.achievement_08:
                    //Hit an opponent 50 times with an Harpoon
                    if (s_HarpoonHits >= 50)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.achievement_09:
                    //Hit an opponent 50 times with an Bubble
                    if (s_BubbleHits >= 50)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.achievement_10:
                    //Hit an opponent 50 times with an Boomerang
                    if (s_BoomerangHits >= 50)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.achievement_11:
                    //Hit an opponent 50 times with an Dynamite
                    if (s_DynamiteHits >= 50)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.achievement_12:
                    //Hit an opponent 50 times with an Implosion
                    if (s_ImplosionHits >= 50)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.achievement_13:
                    //Hit an opponent 50 times with an Disc
                    if (s_DiscHits >= 50)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.achievement_14:
                    //Win a match on the Beach level as a lifeguard shark
                    if (s_WinBeachLifeguard == 1)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.achievement_15:
                    //Win a match on the Pirate ship level as a pirate shark
                    if (s_WinPirateShipPirate == 1)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.achievement_16:
                    //Win a match on the Castle level as a knight shark.
                    if (s_WinCastleKnight == 1)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.achievement_17:
                    //Win a match on the Volcano level as a tiki shark.
                    if (s_WinVolcanoTiki == 1)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.achievement_18:
                    //Win a match on the Space station level as an astronaut shark
                    if (s_WinSpaceStationAstronaut == 1)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.achievement_19:
                    //Win a match on the Temple level as an archaeologist shark.
                    if (s_WinTempleArcheologist == 1)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.achievement_20:
                    // Pickup Every Powerup
                    if (s_PickUpAllPowerups == 10)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.achievement_21:
                    //Get hit by every hazard.
                    if (s_GetHitByAllHazards == 10)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.achievement_22:
                    //Get hit by a boulder in the Temple level as the archeologist shark.
                    if (s_GetHitByBoulderAsArcheologist == 1)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.achievement_23:
                    //Get hit by the barrage in the Castle level as the knight shark.
                    if (s_GetHitByBarrageAsKnight == 1)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.achievement_24:
                    //Get hit by a wave in the Beach level as the lifeguard shark.
                    if (s_GetHitByWaveAsLifeguard == 1)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.achievement_25:
                    //Get hit by a cannon in the Pirate ship level as the pirate shark
                    if (s_GetHitByCannonAsPirate == 1)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.achievement_26:
                    //Get sucked out of the Space station as the astronaut shark
                    if (s_GetSuckedOutAsAstronaut == 1)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.achievement_27:
                    //Be the first to get eliminated in the Volcano level as the tiki shark
                    if (s_FirstInLavaAsTiki == 1)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.achievement_28:
                    //Using the fishing pole, hook an opponent that’s airborne.
                    if (s_HookSomeoneAirborne == 1)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                default:
                    break;
            }
        }

        // Store stats in Steam database if necessary
        if (m_StoreStats)
        {
            // Already saved achievements in UnlockAchievements

            // Set stats

            // Usable Hits
            SteamUserStats.SetStat("HarpoonHits", s_HarpoonHits);
            SteamUserStats.SetStat("AnchorHits", s_AnchorHits);
            SteamUserStats.SetStat("DiscHits", s_DiscHits);
            SteamUserStats.SetStat("BubbleHits", s_BubbleHits);
            SteamUserStats.SetStat("BoomerangHits", s_BoomerangHits);
            SteamUserStats.SetStat("DynamiteHits", s_DynamiteHits);
            SteamUserStats.SetStat("ImplosionHits", s_ImplosionHits);
            // Pickup all powerups
            SteamUserStats.SetStat("PickUpAnchor", s_PickUpAnchor);
            SteamUserStats.SetStat("PickUpShipHarpoon", s_PickUpShipHarpoon);
            SteamUserStats.SetStat("PickUpSpaceHarpoon", s_PickUpSpaceHarpoon);
            SteamUserStats.SetStat("PickUpShieldDisc", s_PickUpShieldDisc);
            SteamUserStats.SetStat("PickUpWheelDisc", s_PickUpWheelDisc);
            SteamUserStats.SetStat("PickUpSpaceDisc", s_PickUpSpaceDisc);
            SteamUserStats.SetStat("PickUpBubble", s_PickUpBubble);
            SteamUserStats.SetStat("PickUpBoomerang", s_PickUpBoomerang);
            SteamUserStats.SetStat("PickUpGrenade", s_PickupDynamite);
            SteamUserStats.SetStat("PickUpTempleImplosion", s_PickUpTempleImplosion);
            SteamUserStats.SetStat("PickUpSpaceImplosion", s_PickUpSpaceImplosion);
            // Win Against Bots
            SteamUserStats.SetStat("AceAgainstBotsNormal", s_AceAgainstBotsNormal);
            SteamUserStats.SetStat("AceAgainstBotsHard", s_AceAgainstBotsHard);
            SteamUserStats.SetStat("TournamentWinBotsNormal", s_TournamentWinBotsNormal);
            SteamUserStats.SetStat("TournamentWinBotsHard", s_TournamentWinBotsHard);
            SteamUserStats.SetStat("SuperTournamentWinBotsNormal", s_SuperTournamentWinBotsNormal);
            SteamUserStats.SetStat("SuperTournamentWinBotsHard", s_SuperTournamentWinBotsHard);
            SteamUserStats.SetStat("AceTournamentAgainstBots", s_AceTournamentAgainstBots);
            // Win as specific skin 
            SteamUserStats.SetStat("WinBeachLifeguard", s_WinBeachLifeguard);
            SteamUserStats.SetStat("WinPirateShipPirate", s_WinPirateShipPirate);
            SteamUserStats.SetStat("WinVolcanoTiki", s_WinVolcanoTiki);
            SteamUserStats.SetStat("WinCastleKnight", s_WinCastleKnight);
            SteamUserStats.SetStat("WinSpaceStationAstronaut", s_WinSpaceStationAstronaut);
            SteamUserStats.SetStat("WinTempleArcheologist", s_WinTempleArcheologist);
            // Pickup all powerups
            SteamUserStats.SetStat("PickUpAnchor", s_PickUpAnchor);
            SteamUserStats.SetStat("PickUpShipHarpoon", s_PickUpShipHarpoon);
            SteamUserStats.SetStat("PickUpSpaceHarpoon", s_PickUpSpaceHarpoon);
            SteamUserStats.SetStat("PickUpShieldDisc", s_PickUpShieldDisc);
            SteamUserStats.SetStat("PickUpWheelDisc", s_PickUpWheelDisc);
            SteamUserStats.SetStat("PickUpSpaceDisc", s_PickUpSpaceDisc);
            SteamUserStats.SetStat("PickUpBubble", s_PickUpBubble);
            SteamUserStats.SetStat("PickUpBoomerang", s_PickUpBoomerang);
            SteamUserStats.SetStat("PickUpGrenade", s_PickupDynamite);
            SteamUserStats.SetStat("PickUpTempleImplosion", s_PickUpTempleImplosion);
            SteamUserStats.SetStat("PickUpSpaceImplosion", s_PickUpSpaceImplosion);
            // Hit By Hazards
            SteamUserStats.SetStat("HitByWave", s_HitByWave);
            SteamUserStats.SetStat("HitByBeachBall", s_HitByBeachBall);
            SteamUserStats.SetStat("HitBySeaMine", s_HitBySeaMine);
            SteamUserStats.SetStat("HitByCannons", s_HitByCannons);
            SteamUserStats.SetStat("HitByBarrage", s_HitByBarrage);
            SteamUserStats.SetStat("HitByBoulder", s_HitByBoulder);
            SteamUserStats.SetStat("HitByPillarDart", s_HitByPillarDart);
            SteamUserStats.SetStat("HitByPressurePlate", s_HitByPressurePlate);
            SteamUserStats.SetStat("HitByAirlock", s_HitByAirlock);
            SteamUserStats.SetStat("HitByGrates", s_HitByGrates);
            // Misc
            SteamUserStats.SetStat("PickUpAllPowerups", s_PickUpAllPowerups);
            SteamUserStats.SetStat("GetHitByAllHazards", s_GetHitByAllHazards);
            SteamUserStats.SetStat("GetHitByBoulderAsArcheologist", s_GetHitByBoulderAsArcheologist);
            SteamUserStats.SetStat("GetHitByBarrageAsKnight", s_GetHitByBarrageAsKnight);
            SteamUserStats.SetStat("GetHitByWaveAsLifeguard", s_GetHitByWaveAsLifeguard);
            SteamUserStats.SetStat("GetSuckedOutAsAstronaut", s_GetSuckedOutAsAstronaut);
            SteamUserStats.SetStat("GetHitByCannonAsPirate", s_GetHitByCannonAsPirate);
            SteamUserStats.SetStat("FirstInLavaAsTiki", s_FirstInLavaAsTiki);
            SteamUserStats.SetStat("HookSomeoneAirborne", s_HookSomeoneAirborne);

            bool success = SteamUserStats.StoreStats();
            // If this failed, we never sent anything to the server, try
            // again later.
            m_StoreStats = !success;
        }
    }

    //-----------------------------------------------------------------------------
    // Purpose: We have stats data from Steam. It is authoritative, so update
    //			our data with those results now.
    //-----------------------------------------------------------------------------
    private void OnUserStatsReceived(UserStatsReceived_t pCallback)
    {
        if (!SteamManager.Initialized)
            return;
        
        // we may get callbacks for other games' stats arriving, ignore them
        if ((ulong)GameID == pCallback.m_nGameID)
        {
            if (EResult.k_EResultOK == pCallback.m_eResult)
            {
                //Debug.Log("Received stats and achievements from Steam\n");

                m_StatsValid = true;

                // load achievements
                foreach (Achievement_t ach in Achievements)
                {
                    bool ret = SteamUserStats.GetAchievement(ach.AchievementID.ToString(), out ach.Achieved);
                    if (ret)
                    {
                        ach.Name = SteamUserStats.GetAchievementDisplayAttribute(ach.AchievementID.ToString(), "name");
                        ach.Description = SteamUserStats.GetAchievementDisplayAttribute(ach.AchievementID.ToString(), "desc");
                    }
                    else
                    {
                        //Debug.LogWarning("SteamUserStats.GetAchievement failed for Achievement " + ach.AchievementID + "\nIs it registered in the Steam Partner site?");
                    }
                }

                // load stats
                // Usables Thrown
                SteamUserStats.GetStat("HarpoonHits", out s_HarpoonHits);
                SteamUserStats.GetStat("AnchorHits", out s_AnchorHits);
                SteamUserStats.GetStat("DiscHits", out s_DiscHits);
                SteamUserStats.GetStat("BubbleHits", out s_BubbleHits);
                SteamUserStats.GetStat("BoomerangHits", out s_BoomerangHits);
                SteamUserStats.GetStat("DynamiteHits", out s_DynamiteHits);
                SteamUserStats.GetStat("ImplosionHits", out s_ImplosionHits);
                // Win Against Bots
                SteamUserStats.GetStat("AceAgainstBotsNormal", out s_AceAgainstBotsNormal);
                SteamUserStats.GetStat("AceAgainstBotsHard", out s_AceAgainstBotsHard);
                SteamUserStats.GetStat("TournamentWinBotsNormal", out s_TournamentWinBotsNormal);
                SteamUserStats.GetStat("TournamentWinBotsHard", out s_TournamentWinBotsHard);
                SteamUserStats.GetStat("SuperTournamentWinBotsNormal", out s_SuperTournamentWinBotsNormal);
                SteamUserStats.GetStat("SuperTournamentWinBotsHard", out s_SuperTournamentWinBotsHard);
                SteamUserStats.GetStat("AceTournamentAgainstBots", out s_AceTournamentAgainstBots);
                // Win as specific skin 
                SteamUserStats.GetStat("WinBeachLifeguard", out s_WinBeachLifeguard);
                SteamUserStats.GetStat("WinPirateShipPirate", out s_WinPirateShipPirate);
                SteamUserStats.GetStat("WinVolcanoTiki", out s_WinVolcanoTiki);
                SteamUserStats.GetStat("WinCastleKnight", out s_WinCastleKnight);
                SteamUserStats.GetStat("WinSpaceStationAstronaut", out s_WinSpaceStationAstronaut);
                SteamUserStats.GetStat("WinTempleArcheologist", out s_WinTempleArcheologist);
                // Pickup all powerups
                SteamUserStats.GetStat("PickUpAnchor", out s_PickUpAnchor);
                SteamUserStats.GetStat("PickUpShipHarpoon", out s_PickUpShipHarpoon);
                SteamUserStats.GetStat("PickUpSpaceHarpoon", out s_PickUpSpaceHarpoon);
                SteamUserStats.GetStat("PickUpShieldDisc", out s_PickUpShieldDisc);
                SteamUserStats.GetStat("PickUpWheelDisc", out s_PickUpWheelDisc);
                SteamUserStats.GetStat("PickUpSpaceDisc", out s_PickUpSpaceDisc);
                SteamUserStats.GetStat("PickUpBubble", out s_PickUpBubble);
                SteamUserStats.GetStat("PickUpBoomerang", out s_PickUpBoomerang);
                SteamUserStats.GetStat("PickUpGrenade", out s_PickupDynamite);
                SteamUserStats.GetStat("PickUpTempleImplosion", out s_PickUpTempleImplosion);
                SteamUserStats.GetStat("PickUpSpaceImplosion", out s_PickUpSpaceImplosion);
                // Hit By Hazards
                SteamUserStats.GetStat("HitByWave", out s_HitByWave);
                SteamUserStats.GetStat("HitByBeachBall", out s_HitByBeachBall);
                SteamUserStats.GetStat("HitBySeaMine", out s_HitBySeaMine);
                SteamUserStats.GetStat("HitByCannons", out s_HitByCannons);
                SteamUserStats.GetStat("HitByBarrage", out s_HitByBarrage);
                SteamUserStats.GetStat("HitByBoulder", out s_HitByBoulder);
                SteamUserStats.GetStat("HitByPillarDart", out s_HitByPillarDart);
                SteamUserStats.GetStat("HitByPressurePlate", out s_HitByPressurePlate);
                SteamUserStats.GetStat("HitByAirlock", out s_HitByAirlock);
                SteamUserStats.GetStat("HitByGrates", out s_HitByGrates);
                // Misc
                SteamUserStats.GetStat("PickUpAllPowerups", out s_PickUpAllPowerups);
                SteamUserStats.GetStat("GetHitByAllHazards", out s_GetHitByAllHazards);
                SteamUserStats.GetStat("GetHitByBoulderAsArcheologist", out s_GetHitByBoulderAsArcheologist);
                SteamUserStats.GetStat("GetHitByBarrageAsKnight", out s_GetHitByBarrageAsKnight);
                SteamUserStats.GetStat("GetHitByWaveAsLifeguard", out s_GetHitByWaveAsLifeguard);
                SteamUserStats.GetStat("GetSuckedOutAsAstronaut", out s_GetSuckedOutAsAstronaut);
                SteamUserStats.GetStat("GetHitByCannonAsPirate", out s_GetHitByCannonAsPirate);
                SteamUserStats.GetStat("FirstInLavaAsTiki", out s_FirstInLavaAsTiki);
                SteamUserStats.GetStat("HookSomeoneAirborne", out s_HookSomeoneAirborne);
            }
            else
            {
                //Debug.Log("RequestStats - failed, " + pCallback.m_eResult);
            }
        }
    }

    //-----------------------------------------------------------------------------
    // Purpose: Our stats data was stored!
    //-----------------------------------------------------------------------------
    private void OnUserStatsStored(UserStatsStored_t pCallback)
    {
        // we may get callbacks for other games' stats arriving, ignore them
        if ((ulong)GameID == pCallback.m_nGameID)
        {
            if (EResult.k_EResultOK == pCallback.m_eResult)
            {
                //Debug.Log("StoreStats - success");
            }
            else if (EResult.k_EResultInvalidParam == pCallback.m_eResult)
            {
                // One or more stats we set broke a constraint. They've been reverted,
                // and we should re-iterate the values now to keep in sync.
                //Debug.Log("StoreStats - some failed to validate");
                // Fake up a callback here so that we re-load the values.
                UserStatsReceived_t callback = new UserStatsReceived_t();
                callback.m_eResult = EResult.k_EResultOK;
                callback.m_nGameID = (ulong)GameID;
                OnUserStatsReceived(callback);
            }
            else
            {
                //Debug.Log("StoreStats - failed, " + pCallback.m_eResult);
            }
        }
    }

    //-----------------------------------------------------------------------------
    // Purpose: An achievement was stored
    //-----------------------------------------------------------------------------
    private void OnUserAchievementStored(UserAchievementStored_t pCallback)
    {
        // We may get callbacks for other games' stats arriving, ignore them
        if ((ulong)GameID == pCallback.m_nGameID)
        {
            if (0 == pCallback.m_nMaxProgress)
            {
                //Debug.Log("Achievement '" + pCallback.m_rgchAchievementName + "' unlocked!");
            }
            else
            {
                //Debug.Log("Achievement '" + pCallback.m_rgchAchievementName + "' progress callback, (" + pCallback.m_nCurProgress + "," + pCallback.m_nMaxProgress + ")");
            }
        }
    }

    private void UnlockSteamAchievement(string pID)
    {
        TestAchievementUnlocked(pID);
        if (!UnlockTest)
        {
            SteamUserStats.SetAchievement(pID);
            SteamUserStats.StoreStats();
        }
    }

    private void UnlockAchievement(Achievement_t pAchievement)
    {
        pAchievement.Achieved = true;

        // mark it down
        SteamUserStats.SetAchievement(pAchievement.AchievementID.ToString());

        // Store stats end of frame
        m_StoreStats = true;
    }

    private void TestAchievementUnlocked(string pID)
    {
        SteamUserStats.GetAchievement(pID, out UnlockTest);
    }

    public bool GetSteamAchievementStatus(string pID)
    {
        TestAchievementUnlocked(pID);
        return UnlockTest;
    }

    public void DEBUG_LockSteamAchievement(string pID)
    {
        TestAchievementUnlocked(pID);
        if (UnlockTest)
        {
            SteamUserStats.ClearAchievement(pID);
        }
    }

    public void DEBUG_KillAllBots()
    {
        var bots = FindObjectsOfType<PlayerController>().Where(a => a.RewiredID == 5);
        foreach (var item in bots)
        {
            GameManager.Instance.KillPlayer(item);
        }
    }

    public void DEBUG_IncrementAllHitStats()
    {
        s_HarpoonHits++;
        s_AnchorHits++;
        s_DiscHits++;
        s_BubbleHits++;
        s_BoomerangHits++;
        s_DynamiteHits++;
        s_ImplosionHits++;
        m_StoreStats = true;
    }

    public void DEBUG_ResetAllStatsAndAchievements()
    {
        //foreach (var achievement in Achievements)
        //{
        //    DEBUG_LockSteamAchievement(achievement.AchievementID.ToString());
        //}
        SteamUserStats.ResetAllStats(true);
    }

    private class Achievement_t
    {
        public Achievement AchievementID;
        public string Name;
        public string Description;
        public bool Achieved;

        /// <summary>
        /// Creates an Achievement. You must also mirror the data provided here in https://partner.steamgames.com/apps/achievements/yourappid
        /// </summary>
        /// <param name="pAchievementID">The "API Name Progress Stat" used to uniquely identify the achievement.</param>
        /// <param name="pName">The "Display Name" that will be shown to players in game and on the Steam Community.</param>
        /// <param name="pDescription">The "Description" that will be shown to players in game and on the Steam Community.</param>
        public Achievement_t(Achievement pAchievementID, string pName, string pDescription)
        {
            AchievementID = pAchievementID;
            Name = pName;
            Description = pDescription;
            Achieved = false;
        }
    }

    private void OnDisable()
    {
        // Unregister Listeners
        GameEndEvent.UnregisterListener(OnGameEndEvent);
        UsableHitEvent.UnregisterListener(OnUsableHitEvent);
        HazardEvent.UnregisterListener(OnHazardEvent);
        PowerupPickupEvent.UnregisterListener(OnPowerupPickupEvent);
    }
}
